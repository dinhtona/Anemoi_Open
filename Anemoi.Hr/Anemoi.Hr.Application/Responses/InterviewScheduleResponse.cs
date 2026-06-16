using System;
using System.Collections.Generic;

namespace Anemoi.Hr.Application.Responses;

public sealed class InterviewScheduleResponse
{
    public string Id { get; set; }
    public string CandidateApplicationId { get; set; }
    public string CandidateName { get; set; }
    public string PostingTitle { get; set; }
    public string CurrentStage { get; set; }
    public string InterviewType { get; set; }
    public DateTime ScheduledAt { get; set; }
    public int DurationMinutes { get; set; }
    public string InterviewerEmployeeId { get; set; }
    public string InterviewerName { get; set; }
    public string Notes { get; set; }
    public string Result { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string UpdatedBy { get; set; }
    public List<InterviewFeedbackResponse> Feedbacks { get; set; } = [];
}

public sealed class InterviewFeedbackResponse
{
    public string Id { get; set; }
    public string InterviewScheduleId { get; set; }
    public string InterviewerEmployeeId { get; set; }
    public string InterviewerName { get; set; }
    public int Rating { get; set; }
    public string Strengths { get; set; }
    public string Concerns { get; set; }
    public string Recommendation { get; set; }
    public DateTime CreatedAt { get; set; }
}
