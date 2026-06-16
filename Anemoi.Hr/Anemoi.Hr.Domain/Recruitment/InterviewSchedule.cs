using Anemoi.BuildingBlock.Domain;
using Anemoi.Hr.Domain.Employees;
using Anemoi.Hr.ModelIds.ModelIds;
using System;
using System.Collections.Generic;

namespace Anemoi.Hr.Domain.Recruitment;

public sealed class InterviewSchedule : ValueObject
{
    public InterviewScheduleId Id { get; set; }
    public CandidateApplicationId CandidateApplicationId { get; set; }
    public string InterviewType { get; set; }
    public DateTime ScheduledAt { get; set; }
    public int DurationMinutes { get; set; }
    public EmployeeId InterviewerEmployeeId { get; set; }
    public string Notes { get; set; }
    public string Result { get; private set; } = InterviewResultCode.Pending;
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string UpdatedBy { get; set; }

    // Navigation
    public CandidateApplication CandidateApplication { get; set; }
    public Employee Interviewer { get; set; }
    public List<InterviewFeedback> Feedbacks { get; set; } = [];

    public bool Reschedule(DateTime newScheduledAt, int durationMinutes, string actor, DateTime now)
    {
        if (InterviewResultCode.IsCompleted(Result))
            return false;
        if (durationMinutes <= 0)
            return false;

        ScheduledAt = newScheduledAt;
        DurationMinutes = durationMinutes;
        UpdatedBy = actor;
        UpdatedAt = now;
        return true;
    }

    public bool MarkResult(string newResult, string actor, DateTime now)
    {
        if (InterviewResultCode.IsCompleted(Result))
            return false;

        if (!InterviewResultCode.CompletedResults.Contains(newResult))
            return false;

        Result = newResult;
        UpdatedBy = actor;
        UpdatedAt = now;
        return true;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Id;
    }
}
