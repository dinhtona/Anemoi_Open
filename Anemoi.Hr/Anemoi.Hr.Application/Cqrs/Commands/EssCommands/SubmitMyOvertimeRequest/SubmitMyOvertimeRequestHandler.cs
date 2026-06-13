using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Events;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Employees;
using Anemoi.Hr.Domain.Overtime;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using OneOf;

namespace Anemoi.Hr.Application.Cqrs.Commands.EssCommands.SubmitMyOvertimeRequest;

public sealed class SubmitMyOvertimeRequestHandler(
    ISqlRepository<Employee> employeeRepository,
    ISqlRepository<OvertimeRequest> overtimeRequestRepository,
    IUnitOfWork unitOfWork,
    IPublishEndpoint publishEndpoint,
    HrSettings hrSettings)
    : ICommandHandler<SubmitMyOvertimeRequestCommand, OneOf<OvertimeRequestIdResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<OvertimeRequestIdResponse, ErrorDetailResponse>> Handle(SubmitMyOvertimeRequestCommand request,
        CancellationToken cancellationToken)
    {
        Employee employee = null;
        if (Guid.TryParse(request.UserId, out var identityUserId))
            employee = await employeeRepository.GetFirstByConditionAsync(
                x => x.IdentityUserId == identityUserId, null, cancellationToken);

        if (employee is null && !string.IsNullOrEmpty(request.Email))
        {
            var searchEmail = request.Email.ToLower();
            employee = await employeeRepository.GetFirstByConditionAsync(
                x => x.WorkEmail != null && x.WorkEmail.ToLower() == searchEmail, null, cancellationToken);
        }

        if (employee is null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.EmployeeNotFound);

        var overtimeDateLimit = DateOnly.FromDateTime(DateTime.Today.AddDays(-hrSettings.OvertimeHistoricalDaysLimit));
        if (request.OvertimeDate < overtimeDateLimit)
            return HrErrorResponses.Create(HrBusinessErrorCodes.OvertimeDateInvalid);

        var overlapping = await overtimeRequestRepository.GetQueryable()
            .Where(x => x.EmployeeId == employee.Id
                && x.OvertimeDate == request.OvertimeDate
                && x.Status == OvertimeStatusCode.Pending
                && x.StartTime < request.EndTime
                && x.EndTime > request.StartTime)
            .AnyAsync(cancellationToken);
        if (overlapping)
            return HrErrorResponses.Create(HrBusinessErrorCodes.OverlappingOvertimeRequestsNotAllowed);

        var approvedOverlapping = await overtimeRequestRepository.GetQueryable()
            .Where(x => x.EmployeeId == employee.Id
                && x.OvertimeDate == request.OvertimeDate
                && x.Status == OvertimeStatusCode.Approved
                && x.StartTime < request.EndTime
                && x.EndTime > request.StartTime)
            .AnyAsync(cancellationToken);
        if (approvedOverlapping)
            return HrErrorResponses.Create(HrBusinessErrorCodes.OverlappingOvertimeRequestsNotAllowed);

        var duration = (request.EndTime.ToTimeSpan() - request.StartTime.ToTimeSpan()).TotalHours;
        if (duration > hrSettings.OvertimeMaxHoursPerRequest)
            return HrErrorResponses.Create(HrBusinessErrorCodes.OvertimeDurationExceedsLimit);

        var overtimeRequest = OvertimeRequest.Create(
            employee.Id,
            request.OvertimeDate,
            request.StartTime,
            request.EndTime,
            request.Reason);

        await overtimeRequestRepository.CreateOneAsync(overtimeRequest, cancellationToken);

        var saveResult = await unitOfWork.SaveChangesAsync(cancellationToken);
        if (saveResult.IsT1)
        {
            return saveResult.AsT1 is DbUpdateConcurrencyException
                ? HrErrorResponses.Create(HrBusinessErrorCodes.OvertimeRequestConcurrencyConflict)
                : HrErrorResponses.Create("HR_SAVE_CHANGES_FAILED");
        }

        await publishEndpoint.Publish(new OvertimeRequestCreatedIntegrationEvent(
            overtimeRequest.Id.Value.ToString(),
            overtimeRequest.EmployeeId.Value.ToString()), cancellationToken);

        return new OvertimeRequestIdResponse
        {
            Id = overtimeRequest.Id.Value,
            CreatedAt = overtimeRequest.CreatedAt
        };
    }
}
