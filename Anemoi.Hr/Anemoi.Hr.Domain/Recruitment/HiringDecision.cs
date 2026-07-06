using Anemoi.BuildingBlock.Domain;
using Anemoi.Hr.ModelIds.ModelIds;
using System;
using System.Collections.Generic;

namespace Anemoi.Hr.Domain.Recruitment;

public sealed class HiringDecision : ValueObject
{
    public HiringDecisionId Id { get; set; }
    public CandidateApplicationId CandidateApplicationId { get; set; }
    public string Decision { get; set; }
    public string DecidedBy { get; set; }
    public DateTime DecidedAt { get; set; }
    public string Notes { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation
    public CandidateApplication CandidateApplication { get; set; }

    public static HiringDecision Create(
        HiringDecisionId id,
        CandidateApplicationId candidateApplicationId,
        string decision,
        string decidedBy,
        DateTime now,
        string notes)
    {
        return new HiringDecision
        {
            Id = id,
            CandidateApplicationId = candidateApplicationId,
            Decision = decision,
            DecidedBy = decidedBy,
            DecidedAt = now,
            Notes = notes,
            CreatedAt = now
        };
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Id;
    }
}
