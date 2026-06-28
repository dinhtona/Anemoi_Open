using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.Hr.Application.Abstractions;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Domain.Employees;
using Anemoi.Hr.Domain.Probation;
using Anemoi.Hr.ModelIds.ModelIds;
using Microsoft.EntityFrameworkCore;

namespace Anemoi.Hr.Application.WorkflowTargetStatusUpdaters;

public sealed class ProbationWorkflowStatusUpdater(
    ISqlRepository<ProbationRecord> probationRepository,
    ISqlRepository<Employee> employeeRepository,
    IUnitOfWork unitOfWork)
    : IWorkflowTargetStatusUpdater
{
    public bool CanHandle(string entityType)
        => entityType == WorkflowConstants.TargetEntityTypes.ProbationRecord;

    public async Task MarkApprovedAsync(string entityId, string performedBy, CancellationToken ct)
    {
        var id = new ProbationRecordId(Guid.Parse(entityId));
        var record = await probationRepository.GetQueryable()
            .FirstOrDefaultAsync(x => x.Id == id, ct);
        if (record is null) return;

        var reviewer = await ResolveReviewerAsync(performedBy, ct);

        record.Pass("Approved", null, reviewer?.Id
            ?? record.ReviewerEmployeeId
            ?? throw new InvalidOperationException("Cannot resolve reviewer for probation approval."));

        await unitOfWork.SaveChangesAsync(ct);
    }

    public async Task MarkRejectedAsync(string entityId, string performedBy, string? reason, CancellationToken ct)
    {
        var id = new ProbationRecordId(Guid.Parse(entityId));
        var record = await probationRepository.GetQueryable()
            .FirstOrDefaultAsync(x => x.Id == id, ct);
        if (record is null) return;

        var reviewer = await ResolveReviewerAsync(performedBy, ct);

        record.Fail(reason ?? "Rejected", reviewer?.Id
            ?? record.ReviewerEmployeeId
            ?? throw new InvalidOperationException("Cannot resolve reviewer for probation rejection."));

        await unitOfWork.SaveChangesAsync(ct);
    }

    private async Task<Employee?> ResolveReviewerAsync(string performedBy, CancellationToken ct)
    {
        if (!Guid.TryParse(performedBy, out var userId)) return null;
        return await employeeRepository.GetQueryable()
            .FirstOrDefaultAsync(e => e.IdentityUserId == userId, ct);
    }
}
