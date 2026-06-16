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

namespace Anemoi.Hr.Application.Cqrs.Commands.LeaveRequestCommands.RejectLeaveRequest;

public sealed class RejectLeaveRequestHandler(
    ISqlRepository<LeaveRequest> leaveRequestRepository,
    ISqlRepository<LeaveBalance> leaveBalanceRepository,
    ISqlRepository<LeaveTransaction> leaveTransactionRepository,
    IUnitOfWork unitOfWork,
    IPublishEndpoint publishEndpoint)
    : ICommandHandler<RejectLeaveRequestCommand, OneOf<None, ErrorDetailResponse>>
{
    public async Task<OneOf<None, ErrorDetailResponse>> Handle(RejectLeaveRequestCommand request,
        CancellationToken cancellationToken)
    {
        var leaveRequest = await leaveRequestRepository.GetFirstByConditionAsync(x => x.Id == request.Id,
            null, cancellationToken);
        if (leaveRequest is null) return HrErrorResponses.Create(HrBusinessErrorCodes.LeaveRequestNotFound);
        if (leaveRequest.StatusCode == LeaveRequestStatusCode.Approved)
            return HrErrorResponses.Create(HrBusinessErrorCodes.LeaveRequestAlreadyApproved);
        if (leaveRequest.StatusCode == LeaveRequestStatusCode.Cancelled)
            return HrErrorResponses.Create(HrBusinessErrorCodes.LeaveRequestAlreadyCancelled);
        if (leaveRequest.StatusCode == LeaveRequestStatusCode.Rejected)
            return HrErrorResponses.Create(HrBusinessErrorCodes.LeaveRequestAlreadyRejected);

        var balance = await ReleasePendingAsync(leaveRequest, request.Comment, leaveBalanceRepository,
            leaveTransactionRepository, cancellationToken);
        if (balance is null) return HrErrorResponses.Create(HrBusinessErrorCodes.LeaveBalanceNotFound);

        leaveRequest.StatusCode = LeaveRequestStatusCode.Rejected;
        leaveRequest.UpdatedAt = DateTime.UtcNow;

        await publishEndpoint.Publish(new LeaveRequestRejectedIntegrationEvent(
            leaveRequest.Id.Value.ToString(),
            leaveRequest.EmployeeId.Value.ToString(),
            leaveRequest.LeavePolicyId.Value.ToString()), cancellationToken);
        await publishEndpoint.Publish(new LeaveBalanceChangedIntegrationEvent(
            balance.EmployeeId.Value.ToString(),
            balance.LeavePolicyId.Value.ToString(),
            balance.Year,
            balance.RemainingDays,
            LeaveBalanceTransactionType.PendingRelease), cancellationToken);

        var saveResult = await unitOfWork.SaveChangesAsync(cancellationToken);
        if (saveResult.IsT1)
            return HrErrorResponses.FromSaveResult(saveResult.AsT1, HrBusinessErrorCodes.LeaveBalanceConcurrencyConflict);

        return None.Value;
    }

    private static async Task<LeaveBalance> ReleasePendingAsync(LeaveRequest request, string reason,
        ISqlRepository<LeaveBalance> balanceRepository,
        ISqlRepository<LeaveTransaction> transactionRepository,
        CancellationToken cancellationToken)
    {
        var balance = await balanceRepository.GetFirstByConditionAsync(
            x => x.EmployeeId == request.EmployeeId && x.LeavePolicyId == request.LeavePolicyId &&
                x.Year == request.StartDate.Year,
            null,
            cancellationToken);
        if (balance is null) return null;

        balance.PendingDays -= request.RequestedDays;
        balance.RemainingDays += request.RequestedDays;
        balance.UpdatedAt = DateTime.UtcNow;
        await transactionRepository.CreateOneAsync(new LeaveTransaction
        {
            Id = new LeaveTransactionId(IdGenerator.NextGuid()),
            EmployeeId = request.EmployeeId,
            LeavePolicyId = request.LeavePolicyId,
            LeaveBalanceId = balance.Id,
            LeaveRequestId = request.Id,
            TransactionTypeCode = LeaveBalanceTransactionType.PendingRelease,
            Days = request.RequestedDays,
            BalanceAfterDays = balance.RemainingDays,
            SourceType = LeaveBalanceTransactionType.SourceTypeLeaveRequest,
            SourceId = request.Id.Value.ToString(),
            Reason = reason,
            CreatedAt = DateTime.UtcNow
        }, cancellationToken);

        return balance;
    }
}
