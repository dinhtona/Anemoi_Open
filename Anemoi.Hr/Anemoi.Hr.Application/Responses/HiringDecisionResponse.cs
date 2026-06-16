using System;

namespace Anemoi.Hr.Application.Responses;

public sealed class HiringDecisionResponse
{
    public string Id { get; set; }
    public string CandidateApplicationId { get; set; }
    public string CandidateName { get; set; }
    public string PostingTitle { get; set; }
    public string CurrentStage { get; set; }
    public string Decision { get; set; }
    public string DecidedBy { get; set; }
    public DateTime DecidedAt { get; set; }
    public string Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}
