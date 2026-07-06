using Anemoi.BuildingBlock.Domain;
using Anemoi.Hr.Domain.Employees;
using Anemoi.Hr.ModelIds.ModelIds;
using System;
using System.Collections.Generic;

namespace Anemoi.Hr.Domain.Recruitment;

public sealed class InterviewFeedback : ValueObject
{
    public InterviewFeedbackId Id { get; set; }
    public InterviewScheduleId InterviewScheduleId { get; set; }
    public EmployeeId InterviewerEmployeeId { get; set; }
    public int Rating { get; set; }
    public string Strengths { get; set; }
    public string Concerns { get; set; }
    public string Recommendation { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation
    public InterviewSchedule InterviewSchedule { get; set; }
    public Employee Interviewer { get; set; }

    public static InterviewFeedback Create(
        InterviewFeedbackId id,
        InterviewScheduleId interviewScheduleId,
        EmployeeId interviewerEmployeeId,
        int rating,
        string strengths,
        string concerns,
        string recommendation,
        DateTime now)
    {
        if (rating < 1 || rating > 5)
            return null;

        if (string.IsNullOrWhiteSpace(recommendation))
            return null;

        return new InterviewFeedback
        {
            Id = id,
            InterviewScheduleId = interviewScheduleId,
            InterviewerEmployeeId = interviewerEmployeeId,
            Rating = rating,
            Strengths = strengths,
            Concerns = concerns,
            Recommendation = recommendation,
            CreatedAt = now
        };
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Id;
    }
}
