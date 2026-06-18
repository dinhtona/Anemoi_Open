using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.BuildingBlock.Application.Helpers;
using Microsoft.EntityFrameworkCore;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Contract.Identity.ModelIds;
using Anemoi.Hr.Application.Abstractions;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Events;
using Anemoi.Contract.Hr.Events;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Overtime;
using Anemoi.Hr.Domain.Employees;
using Anemoi.Hr.ModelIds.ModelIds;
using MassTransit;
using OneOf;

namespace Anemoi.Hr.Application.Cqrs.Commands.OvertimeRequestCommands.CreateOvertimeRequest;

public sealed class CreateOvertimeRequestHandler(
    ISqlRepository<OvertimeRequest> overtimeRequestRepository,
    ISqlRepository<Employee> employeeRepository,
    IUnitOfWork unitOfWork,
    IPublishEndpoint publishEndpoint,
    IWorkflowEngine workflowEngine,
    ICurrentUser currentUser,
    OvertimeMapper mapper,
    HrSettings hrSettings)
    : ICommandHandler<CreateOvertimeRequestCommand, OneOf<OvertimeRequestIdResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<OvertimeRequestIdResponse, ErrorDetailResponse>> Handle(CreateOvertimeRequestCommand request,
        CancellationToken cancellationToken)
    {
        var employee = await employeeRepository.GetFirstByConditionAsync(x => x.Id == request.EmployeeId, null, cancellationToken);
        if (employee is null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.EmployeeNotFound);

        var overtimeDateLimit = DateOnly.FromDateTime(DateTime.Today.AddDays(-hrSettings.OvertimeHistoricalDaysLimit));
        if (request.OvertimeDate < overtimeDateLimit)
            return HrErrorResponses.Create(HrBusinessErrorCodes.OvertimeDateInvalid);

        var overlapping = await overtimeRequestRepository.GetQueryable()
            .Where(x => x.EmployeeId == request.EmployeeId
                && x.OvertimeDate == request.OvertimeDate
                && x.Status == OvertimeStatusCode.Pending
                && x.StartTime < request.EndTime
                && x.EndTime > request.StartTime)
            .AnyAsync(cancellationToken);
        if (overlapping)
            return HrErrorResponses.Create(HrBusinessErrorCodes.OverlappingOvertimeRequestsNotAllowed);

        var approvedOverlapping = await overtimeRequestRepository.GetQueryable()
            .Where(x => x.EmployeeId == request.EmployeeId
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
            new OvertimeRequestId(IdGenerator.NextGuid()),
            request.EmployeeId,
            request.OvertimeDate,
            request.StartTime,
            request.EndTime,
            request.Reason);

        await overtimeRequestRepository.CreateOneAsync(overtimeRequest, cancellationToken);

        var requesterUserId = new UserId(Guid.Parse(currentUser.UserId));
        await workflowEngine.StartAsync(
            WorkflowConstants.TargetEntityTypes.OvertimeRequest,
            overtimeRequest.Id.Value,
            request.EmployeeId,
            requesterUserId,
            requesterUserId,
            cancellationToken);

        await publishEndpoint.Publish(new OvertimeRequestCreatedIntegrationEvent(
            overtimeRequest.Id.Value.ToString(),
            overtimeRequest.EmployeeId.Value.ToString(),
            employee.DirectManagerEmployeeId?.Value.ToString()), cancellationToken);

        var saveResult = await unitOfWork.SaveChangesAsync(cancellationToken);
        if (saveResult.IsT1)
            return HrErrorResponses.FromSaveResult(saveResult.AsT1, HrBusinessErrorCodes.OvertimeRequestConcurrencyConflict);

        return mapper.ToOvertimeRequestIdResponse(overtimeRequest);
    }
}
