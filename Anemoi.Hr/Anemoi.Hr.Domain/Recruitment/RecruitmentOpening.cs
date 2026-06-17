using Anemoi.BuildingBlock.Domain;
using Anemoi.Hr.ModelIds.ModelIds;
using System;

namespace Anemoi.Hr.Domain.Recruitment;

public sealed class RecruitmentOpening : Entity<RecruitmentOpeningId>
{
    public RecruitmentRequestId RecruitmentRequestId { get; set; }
    public string Code { get; set; }
    public int PlannedHeadcount { get; set; }
    public int FilledHeadcount { get; set; }
    public int RemainingHeadcount => PlannedHeadcount - FilledHeadcount;
    public string Status { get; set; }
    public DateTime OpenedAt { get; set; }
    public DateTime? ClosedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public RecruitmentRequest RecruitmentRequest { get; set; }

    public void MarkFilled(int count)
    {
        FilledHeadcount += count;
        if (FilledHeadcount >= PlannedHeadcount)
        {
            Status = "Filled";
            ClosedAt = DateTime.UtcNow;
        }
    }
}
