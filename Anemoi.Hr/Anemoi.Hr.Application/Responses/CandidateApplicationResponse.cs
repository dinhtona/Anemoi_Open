using System;
using System.Collections.Generic;

namespace Anemoi.Hr.Application.Responses;

public sealed class CandidateApplicationResponse
{
    public string Id { get; set; }
    public string CandidateId { get; set; }
    public string CandidateName { get; set; }
    public string CandidateEmail { get; set; }
    public string JobPostingId { get; set; }
    public string PostingTitle { get; set; }
    public string RequisitionCode { get; set; }
    public DateTime AppliedAt { get; set; }
    public string CurrentStage { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<CandidateApplicationStageHistoryResponse> StageHistories { get; set; } = [];
}

public sealed class CandidateApplicationStageHistoryResponse
{
    public string Id { get; set; }
    public string CandidateApplicationId { get; set; }
    public string FromStage { get; set; }
    public string ToStage { get; set; }
    public string ChangedBy { get; set; }
    public DateTime ChangedAt { get; set; }
    public string Note { get; set; }
}
