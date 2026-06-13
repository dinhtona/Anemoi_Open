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

namespace Anemoi.Hr.Application.Cqrs.Commands.EssCommands.CancelMyOvertimeRequest;

public sealed class CancelMyOvertimeRequestHandler(
    ISqlRepository<Employee> employeeRepository,
    ISqlRepository<OvertimeRequest> overtimeRequestRepository,
    IUnitOfWork unitOfWork,
    IPublishEndpoint publishEndpoint)
    : ICommandHandler<CancelMyOvertimeRequestCommand, OneOf<SuccessResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<SuccessResponse, ErrorDetailResponse>> Handle(CancelMyOvertimeRequestCommand request,
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

        var overtimeRequest = await overtimeRequestRepository.GetFirstByConditionAsync(
            x => x.Id == request.OvertimeRequestId && x.EmployeeId == employee.Id,
            null,
            cancellationToken);
        if (overtimeRequest is null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.OvertimeRequestNotFound);

        overtimeRequest.Cancel(employee.Id.Value.ToString());

        var saveResult = await unitOfWork.SaveChangesAsync(cancellationToken);
        if (saveResult.IsT1)
        {
            return saveResult.AsT1 is DbUpdateConcurrencyException
                ? HrErrorResponses.Create(HrBusinessErrorCodes.OvertimeRequestConcurrencyConflict)
                : HrErrorResponses.Create("HR_SAVE_CHANGES_FAILED");
        }

        await publishEndpoint.Publish(new OvertimeRequestCancelledIntegrationEvent(
            overtimeRequest.Id.Value.ToString(),
            overtimeRequest.EmployeeId.Value.ToString()), cancellationToken);

        return new SuccessResponse();
    }
}
