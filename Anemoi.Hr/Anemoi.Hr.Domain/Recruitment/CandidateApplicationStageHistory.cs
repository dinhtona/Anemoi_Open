using Anemoi.BuildingBlock.Domain;
using Anemoi.Hr.ModelIds.ModelIds;
using System;
using System.Collections.Generic;

namespace Anemoi.Hr.Domain.Recruitment;

public sealed class CandidateApplicationStageHistory : ValueObject
{
    public CandidateApplicationStageHistoryId Id { get; set; }
    public CandidateApplicationId CandidateApplicationId { get; set; }
    public string FromStage { get; set; }
    public string ToStage { get; set; }
    public string ChangedBy { get; set; }
    public DateTime ChangedAt { get; set; }
    public string Note { get; set; }

    // Navigation
    public CandidateApplication CandidateApplication { get; set; }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Id;
    }
}
