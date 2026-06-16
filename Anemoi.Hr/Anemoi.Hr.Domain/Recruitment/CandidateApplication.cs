using Anemoi.BuildingBlock.Domain;
using Anemoi.Hr.ModelIds.ModelIds;
using System;
using System.Collections.Generic;

namespace Anemoi.Hr.Domain.Recruitment;

public sealed class CandidateApplication : ValueObject
{
    public CandidateApplicationId Id { get; set; }
    public CandidateId CandidateId { get; set; }
    public JobPostingId JobPostingId { get; set; }
    public DateTime AppliedAt { get; set; }
    public string CurrentStage { get; private set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation
    public Candidate Candidate { get; set; }
    public JobPosting JobPosting { get; set; }
    public List<CandidateApplicationStageHistory> StageHistories { get; set; } = [];

    public static CandidateApplication Create(
        CandidateApplicationId id,
        CandidateId candidateId,
        JobPostingId jobPostingId,
        DateTime now)
    {
        return new CandidateApplication
        {
            Id = id,
            CandidateId = candidateId,
            JobPostingId = jobPostingId,
            AppliedAt = now,
            CreatedAt = now,
            UpdatedAt = now
        };
    }

    public bool MoveToStage(string toStage, CandidateApplicationStageHistoryId historyId, string actor, DateTime now, string note)
    {
        if (CandidateApplicationStageCode.IsTerminal(CurrentStage))
            return false;

        if (string.IsNullOrWhiteSpace(actor))
            return false;

        // Allow initial Applied transition with null fromStage
        if (CurrentStage == null && toStage == CandidateApplicationStageCode.Applied)
        {
            CurrentStage = toStage;
            UpdatedAt = now;
            StageHistories.Add(new CandidateApplicationStageHistory
            {
                Id = historyId,
                CandidateApplicationId = Id,
                FromStage = null,
                ToStage = toStage,
                ChangedBy = actor,
                ChangedAt = now,
                Note = note
            });
            return true;
        }

        if (!CandidateApplicationStageCode.IsValidTransition(CurrentStage, toStage))
            return false;

        var fromStage = CurrentStage;
        CurrentStage = toStage;
        UpdatedAt = now;
        StageHistories.Add(new CandidateApplicationStageHistory
        {
            Id = historyId,
            CandidateApplicationId = Id,
            FromStage = fromStage,
            ToStage = toStage,
            ChangedBy = actor,
            ChangedAt = now,
            Note = note
        });
        return true;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Id;
    }
}
