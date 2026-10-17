using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Abstractions;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Domain.Payroll;
using Anemoi.Hr.ModelIds.ModelIds;
using Microsoft.EntityFrameworkCore;
using OneOf;

namespace Anemoi.Hr.Application.WorkflowTargetStatusUpdaters;

public sealed class PayrollWorkflowStatusUpdater(
    ISqlRepository<PayrollRun> payrollRepository,
    IUnitOfWork unitOfWork)
    : IWorkflowTargetStatusUpdater
{
    public bool CanHandle(string entityType)
        => entityType == WorkflowConstants.TargetEntityTypes.PayrollRun;

    public async Task<OneOf<bool, ErrorDetailResponse>> MarkApprovedAsync(string entityId, string performedBy, CancellationToken ct)
    {
        var id = new PayrollRunId(Guid.Parse(entityId));
        var payroll = await payrollRepository.GetQueryable()
            .FirstOrDefaultAsync(x => x.Id == id, ct);
        if (payroll is null) return true;
        payroll.Approve(performedBy, DateTime.UtcNow);
        await unitOfWork.SaveChangesAsync(ct);
        return true;
    }

    public async Task<OneOf<bool, ErrorDetailResponse>> MarkRejectedAsync(string entityId, string performedBy, string? reason, CancellationToken ct)
    {
        var id = new PayrollRunId(Guid.Parse(entityId));
        var payroll = await payrollRepository.GetQueryable()
            .FirstOrDefaultAsync(x => x.Id == id, ct);
        if (payroll is null) return true;
        payroll.Reject(performedBy, DateTime.UtcNow, reason ?? string.Empty);
        await unitOfWork.SaveChangesAsync(ct);
        return true;
    }
}
