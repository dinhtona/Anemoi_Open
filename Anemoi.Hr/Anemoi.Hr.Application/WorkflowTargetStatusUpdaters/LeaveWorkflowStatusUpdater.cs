using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Helpers;
using Anemoi.Hr.Application.Abstractions;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Events;
using Anemoi.Contract.Hr.Events;
using Anemoi.Hr.Domain.Leaves;
using Anemoi.Hr.ModelIds.ModelIds;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace Anemoi.Hr.Application.WorkflowTargetStatusUpdaters;

public sealed class LeaveWorkflowStatusUpdater(
    ISqlRepository<LeaveRequest> leaveRepository,
    ISqlRepository<LeaveBalance> leaveBalanceRepository,
    ISqlRepository<LeaveTransaction> leaveTransactionRepository,
    IUnitOfWork unitOfWork,
    IPublishEndpoint publishEndpoint)
    : IWorkflowTargetStatusUpdater
{
    public bool CanHandle(string entityType)
        => entityType == WorkflowConstants.TargetEntityTypes.LeaveRequest;

    public async Task MarkApprovedAsync(string entityId, string performedBy, CancellationToken ct)
    {
        var id = new LeaveRequestId(Guid.Parse(entityId));
        var leave = await leaveRepository.GetQueryable()
            .FirstOrDefaultAsync(x => x.Id == id, ct);
        if (leave is null) return;

        var balance = await leaveBalanceRepository.GetFirstByConditionAsync(
            x => x.EmployeeId == leave.EmployeeId &&
                x.LeavePolicyId == leave.LeavePolicyId &&
                x.Year == leave.StartDate.Year,
            null, ct);

        if (balance is null)
        {
            Log.Warning("LeaveBalance not found for employee {EmployeeId}, policy {PolicyId}, year {Year}",
                leave.EmployeeId, leave.LeavePolicyId, leave.StartDate.Year);
        }
        else
        {
            balance.PendingDays -= leave.RequestedDays;
            balance.UsedDays += leave.RequestedDays;
            balance.UpdatedAt = DateTime.UtcNow;

            await leaveTransactionRepository.CreateManyAsync([
                NewTransaction(leave, balance, LeaveBalanceTransactionType.PendingRelease, leave.RequestedDays),
                NewTransaction(leave, balance, LeaveBalanceTransactionType.Used, leave.RequestedDays)
            ], ct);
        }

        leave.MarkWorkflowApproved(performedBy);
        leave.UpdatedAt = DateTime.UtcNow;

        await publishEndpoint.Publish(new LeaveRequestApprovedIntegrationEvent(
            leave.Id.Value.ToString(),
            leave.EmployeeId.Value.ToString(),
            leave.LeavePolicyId.Value.ToString()), ct);

        if (balance is not null)
        {
            await publishEndpoint.Publish(new LeaveBalanceChangedIntegrationEvent(
                balance.EmployeeId.Value.ToString(),
                balance.LeavePolicyId.Value.ToString(),
                balance.Year,
                balance.RemainingDays,
                LeaveBalanceTransactionType.Used), ct);
        }

        await unitOfWork.SaveChangesAsync(ct);
    }

    public async Task MarkRejectedAsync(string entityId, string performedBy, string? reason, CancellationToken ct)
    {
        var id = new LeaveRequestId(Guid.Parse(entityId));
        var leave = await leaveRepository.GetQueryable()
            .FirstOrDefaultAsync(x => x.Id == id, ct);
        if (leave is null) return;

        var balance = await leaveBalanceRepository.GetFirstByConditionAsync(
            x => x.EmployeeId == leave.EmployeeId &&
                x.LeavePolicyId == leave.LeavePolicyId &&
                x.Year == leave.StartDate.Year,
            null, ct);

        if (balance is null)
        {
            Log.Warning("LeaveBalance not found for employee {EmployeeId}, policy {PolicyId}, year {Year}",
                leave.EmployeeId, leave.LeavePolicyId, leave.StartDate.Year);
        }
        else
        {
            balance.PendingDays -= leave.RequestedDays;
            balance.RemainingDays += leave.RequestedDays;
            balance.UpdatedAt = DateTime.UtcNow;

            await leaveTransactionRepository.CreateOneAsync(new LeaveTransaction
            {
                Id = new LeaveTransactionId(IdGenerator.NextGuid()),
                EmployeeId = leave.EmployeeId,
                LeavePolicyId = leave.LeavePolicyId,
                LeaveBalanceId = balance.Id,
                LeaveRequestId = leave.Id,
                TransactionTypeCode = LeaveBalanceTransactionType.PendingRelease,
                Days = leave.RequestedDays,
                BalanceAfterDays = balance.RemainingDays,
                SourceType = LeaveBalanceTransactionType.SourceTypeLeaveRequest,
                SourceId = leave.Id.Value.ToString(),
                Reason = reason,
                CreatedAt = DateTime.UtcNow
            }, ct);
        }

        leave.MarkWorkflowRejected(performedBy, reason);
        leave.UpdatedAt = DateTime.UtcNow;

        await publishEndpoint.Publish(new LeaveRequestRejectedIntegrationEvent(
            leave.Id.Value.ToString(),
            leave.EmployeeId.Value.ToString(),
            leave.LeavePolicyId.Value.ToString()), ct);

        if (balance is not null)
        {
            await publishEndpoint.Publish(new LeaveBalanceChangedIntegrationEvent(
                balance.EmployeeId.Value.ToString(),
                balance.LeavePolicyId.Value.ToString(),
                balance.Year,
                balance.RemainingDays,
                LeaveBalanceTransactionType.PendingRelease), ct);
        }

        await unitOfWork.SaveChangesAsync(ct);
    }

    private static LeaveTransaction NewTransaction(LeaveRequest request, LeaveBalance balance, string type, decimal days)
    {
        return new LeaveTransaction
        {
            Id = new LeaveTransactionId(IdGenerator.NextGuid()),
            EmployeeId = request.EmployeeId,
            LeavePolicyId = request.LeavePolicyId,
            LeaveBalanceId = balance.Id,
            LeaveRequestId = request.Id,
            TransactionTypeCode = type,
            Days = days,
            BalanceAfterDays = balance.RemainingDays,
            SourceType = LeaveBalanceTransactionType.SourceTypeLeaveRequest,
            SourceId = request.Id.Value.ToString(),
            CreatedAt = DateTime.UtcNow
        };
    }
}
