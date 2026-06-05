using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Microsoft.EntityFrameworkCore;
using Anemoi.BuildingBlock.Application.Helpers;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.BuildingBlock.Application.Results;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Events;
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
        if (leaveRequest.StatusCode == "Approved")
            return HrErrorResponses.Create(HrBusinessErrorCodes.LeaveRequestAlreadyApproved);
        if (leaveRequest.StatusCode == "Cancelled")
            return HrErrorResponses.Create(HrBusinessErrorCodes.LeaveRequestAlreadyCancelled);
        if (leaveRequest.StatusCode == "Rejected")
            return HrErrorResponses.Create(HrBusinessErrorCodes.LeaveRequestAlreadyRejected);

        var balance = await ReleasePendingAsync(leaveRequest, request.Comment, leaveBalanceRepository,
            leaveTransactionRepository, cancellationToken);
        if (balance is null) return HrErrorResponses.Create(HrBusinessErrorCodes.LeaveBalanceNotFound);

        leaveRequest.StatusCode = "Rejected";
        leaveRequest.UpdatedAt = DateTime.UtcNow;

        var saveResult = await unitOfWork.SaveChangesAsync(cancellationToken);
        if (saveResult.IsT1)
        {
            return saveResult.AsT1 is DbUpdateConcurrencyException
                ? HrErrorResponses.Create(HrBusinessErrorCodes.LeaveBalanceConcurrencyConflict)
                : HrErrorResponses.Create("HR_SAVE_CHANGES_FAILED");
        }

        await publishEndpoint.Publish(new LeaveBalanceChangedIntegrationEvent(
            balance.EmployeeId.Value.ToString(),
            balance.LeavePolicyId.Value.ToString(),
            balance.Year,
            balance.RemainingDays,
            "PendingRelease"), cancellationToken);

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
            TransactionTypeCode = "PendingRelease",
            Days = request.RequestedDays,
            BalanceAfterDays = balance.RemainingDays,
            SourceType = "LeaveRequest",
            SourceId = request.Id.Value.ToString(),
            Reason = reason,
            CreatedAt = DateTime.UtcNow
        }, cancellationToken);

        return balance;
    }
}
