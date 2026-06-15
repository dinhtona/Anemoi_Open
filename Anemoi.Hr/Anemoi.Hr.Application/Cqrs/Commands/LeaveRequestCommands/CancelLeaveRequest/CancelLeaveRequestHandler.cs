using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Microsoft.EntityFrameworkCore;
using Anemoi.BuildingBlock.Application.Helpers;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.BuildingBlock.Application.Results;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Events;
using Anemoi.Contract.Hr.Events;
using Anemoi.Hr.Domain.Leaves;
using Anemoi.Hr.ModelIds.ModelIds;
using MassTransit;
using OneOf;

namespace Anemoi.Hr.Application.Cqrs.Commands.LeaveRequestCommands.CancelLeaveRequest;

public sealed class CancelLeaveRequestHandler(
    ISqlRepository<LeaveRequest> leaveRequestRepository,
    ISqlRepository<LeaveBalance> leaveBalanceRepository,
    ISqlRepository<LeaveTransaction> leaveTransactionRepository,
    IUnitOfWork unitOfWork,
    IPublishEndpoint publishEndpoint)
    : ICommandHandler<CancelLeaveRequestCommand, OneOf<None, ErrorDetailResponse>>
{
    public async Task<OneOf<None, ErrorDetailResponse>> Handle(CancelLeaveRequestCommand request,
        CancellationToken cancellationToken)
    {
        var leaveRequest = await leaveRequestRepository.GetFirstByConditionAsync(x => x.Id == request.Id,
            null, cancellationToken);
        if (leaveRequest is null) return HrErrorResponses.Create(HrBusinessErrorCodes.LeaveRequestNotFound);
        if (leaveRequest.StatusCode == LeaveRequestStatusCode.Cancelled)
            return HrErrorResponses.Create(HrBusinessErrorCodes.LeaveRequestAlreadyCancelled);
        if (leaveRequest.StatusCode == LeaveRequestStatusCode.Rejected)
            return HrErrorResponses.Create(HrBusinessErrorCodes.LeaveRequestAlreadyRejected);
        if (leaveRequest.StatusCode is not (LeaveRequestStatusCode.Pending or LeaveRequestStatusCode.Approved))
            return HrErrorResponses.Create(HrBusinessErrorCodes.LeaveRequestInvalidStatus);

        var balance = await leaveBalanceRepository.GetFirstByConditionAsync(
            x => x.EmployeeId == leaveRequest.EmployeeId &&
                x.LeavePolicyId == leaveRequest.LeavePolicyId &&
                x.Year == leaveRequest.StartDate.Year,
            null,
            cancellationToken);
        if (balance is null) return HrErrorResponses.Create(HrBusinessErrorCodes.LeaveBalanceNotFound);

        var transactionType = leaveRequest.StatusCode == LeaveRequestStatusCode.Approved
            ? LeaveBalanceTransactionType.Refund
            : LeaveBalanceTransactionType.PendingRelease;
        if (leaveRequest.StatusCode == LeaveRequestStatusCode.Approved)
        {
            balance.UsedDays -= leaveRequest.RequestedDays;
            balance.RemainingDays += leaveRequest.RequestedDays;
        }
        else
        {
            balance.PendingDays -= leaveRequest.RequestedDays;
            balance.RemainingDays += leaveRequest.RequestedDays;
        }

        balance.UpdatedAt = DateTime.UtcNow;
        leaveRequest.StatusCode = LeaveRequestStatusCode.Cancelled;
        leaveRequest.UpdatedAt = DateTime.UtcNow;
        await leaveTransactionRepository.CreateOneAsync(NewTransaction(leaveRequest, balance, transactionType,
            request.Reason), cancellationToken);

        var saveResult = await unitOfWork.SaveChangesAsync(cancellationToken);
        if (saveResult.IsT1)
        {
            return saveResult.AsT1 is DbUpdateConcurrencyException
                ? HrErrorResponses.Create(HrBusinessErrorCodes.LeaveBalanceConcurrencyConflict)
                : HrErrorResponses.Create(HrBusinessErrorCodes.SaveChangesFailed);
        }

        await publishEndpoint.Publish(new LeaveRequestCancelledIntegrationEvent(
            leaveRequest.Id.Value.ToString(),
            leaveRequest.EmployeeId.Value.ToString(),
            leaveRequest.LeavePolicyId.Value.ToString()), cancellationToken);
        await publishEndpoint.Publish(new LeaveBalanceChangedIntegrationEvent(
            balance.EmployeeId.Value.ToString(),
            balance.LeavePolicyId.Value.ToString(),
            balance.Year,
            balance.RemainingDays,
            transactionType), cancellationToken);

        return None.Value;
    }

    private static LeaveTransaction NewTransaction(LeaveRequest request, LeaveBalance balance, string type, string reason)
    {
        return new LeaveTransaction
        {
            Id = new LeaveTransactionId(IdGenerator.NextGuid()),
            EmployeeId = request.EmployeeId,
            LeavePolicyId = request.LeavePolicyId,
            LeaveBalanceId = balance.Id,
            LeaveRequestId = request.Id,
            TransactionTypeCode = type,
            Days = request.RequestedDays,
            BalanceAfterDays = balance.RemainingDays,
            SourceType = LeaveBalanceTransactionType.SourceTypeLeaveRequest,
            SourceId = request.Id.Value.ToString(),
            Reason = reason,
            CreatedAt = DateTime.UtcNow
        };
    }
}
