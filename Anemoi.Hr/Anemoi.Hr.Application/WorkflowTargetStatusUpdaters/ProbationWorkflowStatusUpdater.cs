using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Abstractions;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Domain.Employees;
using Anemoi.Hr.Domain.Probation;
using Anemoi.Hr.ModelIds.ModelIds;
using Microsoft.EntityFrameworkCore;
using OneOf;

namespace Anemoi.Hr.Application.WorkflowTargetStatusUpdaters;

public sealed class ProbationWorkflowStatusUpdater(
    ISqlRepository<ProbationRecord> probationRepository,
    ISqlRepository<Employee> employeeRepository,
    IUnitOfWork unitOfWork)
    : IWorkflowTargetStatusUpdater
{
    public bool CanHandle(string entityType)
        => entityType == WorkflowConstants.TargetEntityTypes.ProbationRecord;

    public async Task<OneOf<bool, ErrorDetailResponse>> MarkApprovedAsync(string entityId, string performedBy, CancellationToken ct)
    {
        var id = new ProbationRecordId(Guid.Parse(entityId));
        var record = await probationRepository.GetQueryable()
            .FirstOrDefaultAsync(x => x.Id == id, ct);
        if (record is null) return true;

        var reviewer = await ResolveReviewerAsync(performedBy, ct);
        var reviewerId = reviewer?.Id ?? record.ReviewerEmployeeId;
        if (reviewerId is null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.ProbationReviewerNotResolved);

        record.Pass(ProbationResultCode.Approved, null, reviewerId);

        await unitOfWork.SaveChangesAsync(ct);
        return true;
    }

    public async Task<OneOf<bool, ErrorDetailResponse>> MarkRejectedAsync(string entityId, string performedBy, string? reason, CancellationToken ct)
    {
        var id = new ProbationRecordId(Guid.Parse(entityId));
        var record = await probationRepository.GetQueryable()
            .FirstOrDefaultAsync(x => x.Id == id, ct);
        if (record is null) return true;

        var reviewer = await ResolveReviewerAsync(performedBy, ct);
        var reviewerId = reviewer?.Id ?? record.ReviewerEmployeeId;
        if (reviewerId is null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.ProbationReviewerNotResolved);

        record.Fail(reason ?? ProbationResultCode.Rejected, reviewerId);

        await unitOfWork.SaveChangesAsync(ct);
        return true;
    }

    private async Task<Employee?> ResolveReviewerAsync(string performedBy, CancellationToken ct)
    {
        if (!Guid.TryParse(performedBy, out var userId)) return null;
        return await employeeRepository.GetQueryable()
            .FirstOrDefaultAsync(e => e.IdentityUserId == userId, ct);
    }
}
