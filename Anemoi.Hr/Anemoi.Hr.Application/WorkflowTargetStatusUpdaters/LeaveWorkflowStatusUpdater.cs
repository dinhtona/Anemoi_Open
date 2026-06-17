using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.Hr.Application.Abstractions;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Domain.Leaves;
using Anemoi.Hr.ModelIds.ModelIds;
using Microsoft.EntityFrameworkCore;

namespace Anemoi.Hr.Application.WorkflowTargetStatusUpdaters;

public sealed class LeaveWorkflowStatusUpdater(
    ISqlRepository<LeaveRequest> leaveRepository)
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
        leave.MarkWorkflowApproved(performedBy);
    }

    public async Task MarkRejectedAsync(string entityId, string performedBy, string? reason, CancellationToken ct)
    {
        var id = new LeaveRequestId(Guid.Parse(entityId));
        var leave = await leaveRepository.GetQueryable()
            .FirstOrDefaultAsync(x => x.Id == id, ct);
        if (leave is null) return;
        leave.MarkWorkflowRejected(performedBy, reason);
    }
}
