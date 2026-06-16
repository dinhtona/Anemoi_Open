using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.BuildingBlock.Application.Helpers;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Events;
using Anemoi.Contract.Hr.Events;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Employees;
using Anemoi.Hr.Domain.Leaves;
using Anemoi.Hr.ModelIds.ModelIds;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using OneOf;

namespace Anemoi.Hr.Application.Cqrs.Commands.EssCommands.SubmitMyLeaveRequest;

public sealed class SubmitMyLeaveRequestHandler(
    ISqlRepository<Employee> employeeRepository,
    ISqlRepository<LeavePolicy> leavePolicyRepository,
    ISqlRepository<LeaveBalance> leaveBalanceRepository,
    ISqlRepository<LeaveRequest> leaveRequestRepository,
    ISqlRepository<LeaveTransaction> leaveTransactionRepository,
    IUnitOfWork unitOfWork,
    IPublishEndpoint publishEndpoint,
    LeaveMapper mapper)
    : ICommandHandler<SubmitMyLeaveRequestCommand, OneOf<LeaveRequestIdResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<LeaveRequestIdResponse, ErrorDetailResponse>> Handle(SubmitMyLeaveRequestCommand request,
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

        var policy = await leavePolicyRepository.GetFirstByConditionAsync(x => x.Id == request.LeavePolicyId,
            null, cancellationToken);
        if (policy is null) return HrErrorResponses.Create(HrBusinessErrorCodes.LeavePolicyNotFound);

        var year = request.StartDate.Year;
        var balance = await leaveBalanceRepository.GetFirstByConditionAsync(
            x => x.EmployeeId == employee.Id && x.LeavePolicyId == request.LeavePolicyId && x.Year == year,
            null,
            cancellationToken);
        if (balance is null) return HrErrorResponses.Create(HrBusinessErrorCodes.LeaveBalanceNotFound);
        if (balance.RemainingDays < request.RequestedDays)
            return HrErrorResponses.Create(HrBusinessErrorCodes.LeaveBalanceNotEnough);

        var isOverlapping = await leaveRequestRepository.ExistByConditionAsync(
            x => x.EmployeeId == employee.Id
                 && (x.StatusCode == LeaveRequestStatusCode.Pending || x.StatusCode == LeaveRequestStatusCode.Approved)
                 && x.StartDate <= request.EndDate
                 && request.StartDate <= x.EndDate,
            cancellationToken);
        if (isOverlapping)
            return HrErrorResponses.Create(HrBusinessErrorCodes.LeaveRequestOverlapping);

        var leaveRequest = new LeaveRequest
        {
            Id = new LeaveRequestId(IdGenerator.NextGuid()),
            EmployeeId = employee.Id,
            LeavePolicyId = request.LeavePolicyId,
            ApproverEmployeeId = request.ApproverEmployeeId,
            LeaveTypeCode = request.LeaveTypeCode,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            RequestedDays = request.RequestedDays,
            StatusCode = LeaveRequestStatusCode.Pending,
            Reason = request.Reason,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        balance.PendingDays += request.RequestedDays;
        balance.RemainingDays -= request.RequestedDays;
        balance.UpdatedAt = DateTime.UtcNow;

        await leaveRequestRepository.CreateOneAsync(leaveRequest, cancellationToken);
        await leaveTransactionRepository.CreateOneAsync(new LeaveTransaction
        {
            Id = new LeaveTransactionId(IdGenerator.NextGuid()),
            EmployeeId = employee.Id,
            LeavePolicyId = request.LeavePolicyId,
            LeaveBalanceId = balance.Id,
            LeaveRequestId = leaveRequest.Id,
            TransactionTypeCode = LeaveBalanceTransactionType.PendingReserve,
            Days = request.RequestedDays,
            BalanceAfterDays = balance.RemainingDays,
            SourceType = LeaveBalanceTransactionType.SourceTypeLeaveRequest,
            SourceId = leaveRequest.Id.Value.ToString(),
            Reason = request.Reason,
            CreatedAt = DateTime.UtcNow
        }, cancellationToken);

        await publishEndpoint.Publish(new LeaveRequestSubmittedIntegrationEvent(
            leaveRequest.Id.Value.ToString(),
            leaveRequest.EmployeeId.Value.ToString(),
            leaveRequest.LeavePolicyId.Value.ToString(),
            leaveRequest.ApproverEmployeeId?.Value.ToString()), cancellationToken);
        await publishEndpoint.Publish(new LeaveBalanceChangedIntegrationEvent(
            balance.EmployeeId.Value.ToString(),
            balance.LeavePolicyId.Value.ToString(),
            balance.Year,
            balance.RemainingDays,
            LeaveBalanceTransactionType.PendingReserve), cancellationToken);

        var saveResult = await unitOfWork.SaveChangesAsync(cancellationToken);
        if (saveResult.IsT1)
            return HrErrorResponses.FromSaveResult(saveResult.AsT1, HrBusinessErrorCodes.LeaveBalanceConcurrencyConflict);

        return mapper.ToLeaveRequestIdResponse(leaveRequest);
    }
}
