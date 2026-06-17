using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.Hr.Application.Abstractions;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Domain.Payroll;
using Anemoi.Hr.ModelIds.ModelIds;
using Microsoft.EntityFrameworkCore;

namespace Anemoi.Hr.Application.WorkflowTargetStatusUpdaters;

public sealed class PayrollWorkflowStatusUpdater(
    ISqlRepository<PayrollRun> payrollRepository)
    : IWorkflowTargetStatusUpdater
{
    public bool CanHandle(string entityType)
        => entityType == WorkflowConstants.TargetEntityTypes.PayrollRun;

    public async Task MarkApprovedAsync(string entityId, string performedBy, CancellationToken ct)
    {
        var id = new PayrollRunId(Guid.Parse(entityId));
        var payroll = await payrollRepository.GetQueryable()
            .FirstOrDefaultAsync(x => x.Id == id, ct);
        if (payroll is null) return;
        payroll.Approve(performedBy, DateTime.UtcNow);
    }

    public async Task MarkRejectedAsync(string entityId, string performedBy, string? reason, CancellationToken ct)
    {
        var id = new PayrollRunId(Guid.Parse(entityId));
        var payroll = await payrollRepository.GetQueryable()
            .FirstOrDefaultAsync(x => x.Id == id, ct);
        if (payroll is null) return;
        payroll.Reject(performedBy, DateTime.UtcNow, reason ?? string.Empty);
    }
}
