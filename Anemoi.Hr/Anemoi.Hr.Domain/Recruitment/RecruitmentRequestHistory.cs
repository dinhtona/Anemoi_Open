using Anemoi.BuildingBlock.Domain;
using Anemoi.Hr.ModelIds.ModelIds;
using System;

namespace Anemoi.Hr.Domain.Recruitment;

public sealed class RecruitmentRequestHistory : Entity<RecruitmentRequestHistoryId>
{
    public RecruitmentRequestId RecruitmentRequestId { get; set; }
    public string ActionCode { get; set; }
    public string OldStatus { get; set; }
    public string NewStatus { get; set; }
    public string Comment { get; set; }
    public string PerformedBy { get; set; }
    public DateTime PerformedAt { get; set; }

    public RecruitmentRequest RecruitmentRequest { get; set; }
}
