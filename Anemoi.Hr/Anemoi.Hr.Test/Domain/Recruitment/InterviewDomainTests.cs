using Anemoi.Hr.Domain.Recruitment;
using Anemoi.Hr.ModelIds.ModelIds;
using FluentAssertions;
using Xunit;

namespace Anemoi.Hr.Test.Domain.Recruitment;

public class InterviewDomainTests
{
    private static InterviewSchedule CreateInterview()
    {
        return new InterviewSchedule
        {
            Id = new InterviewScheduleId(Guid.NewGuid()),
            CandidateApplicationId = new CandidateApplicationId(Guid.NewGuid()),
            InterviewType = InterviewTypeCode.Onsite,
            ScheduledAt = DateTime.UtcNow.AddDays(7),
            DurationMinutes = 60,
            InterviewerEmployeeId = new EmployeeId(Guid.NewGuid()),
            Notes = "First round",
            CreatedBy = "user1",
            CreatedAt = DateTime.UtcNow,
            UpdatedBy = "user1",
            UpdatedAt = DateTime.UtcNow
        };
    }

    [Fact]
    public void CreateInterview_DefaultResult_ShouldBePending()
    {
        var interview = CreateInterview();
        interview.Result.Should().Be(InterviewResultCode.Pending);
    }

    [Fact]
    public void Reschedule_PendingInterview_ShouldSucceed()
    {
        var interview = CreateInterview();
        var newTime = DateTime.UtcNow.AddDays(14);
        var result = interview.Reschedule(newTime, 90, "user1", DateTime.UtcNow);
        result.Should().BeTrue();
        interview.ScheduledAt.Should().Be(newTime);
        interview.DurationMinutes.Should().Be(90);
    }

    [Fact]
    public void Reschedule_CompletedInterview_ShouldFail()
    {
        var interview = CreateInterview();
        interview.MarkResult(InterviewResultCode.Passed, "user1", DateTime.UtcNow);
        var result = interview.Reschedule(DateTime.UtcNow.AddDays(14), 90, "user1", DateTime.UtcNow);
        result.Should().BeFalse();
    }

    [Fact]
    public void Reschedule_InvalidDuration_ShouldFail()
    {
        var interview = CreateInterview();
        var result = interview.Reschedule(DateTime.UtcNow.AddDays(14), 0, "user1", DateTime.UtcNow);
        result.Should().BeFalse();
    }

    [Fact]
    public void MarkPassed_ShouldSucceed()
    {
        var interview = CreateInterview();
        var result = interview.MarkResult(InterviewResultCode.Passed, "user1", DateTime.UtcNow);
        result.Should().BeTrue();
        interview.Result.Should().Be(InterviewResultCode.Passed);
    }

    [Fact]
    public void MarkFailed_ShouldSucceed()
    {
        var interview = CreateInterview();
        var result = interview.MarkResult(InterviewResultCode.Failed, "user1", DateTime.UtcNow);
        result.Should().BeTrue();
        interview.Result.Should().Be(InterviewResultCode.Failed);
    }

    [Fact]
    public void MarkNoShow_ShouldSucceed()
    {
        var interview = CreateInterview();
        var result = interview.MarkResult(InterviewResultCode.NoShow, "user1", DateTime.UtcNow);
        result.Should().BeTrue();
        interview.Result.Should().Be(InterviewResultCode.NoShow);
    }

    [Fact]
    public void MarkResult_Twice_ShouldFail()
    {
        var interview = CreateInterview();
        interview.MarkResult(InterviewResultCode.Passed, "user1", DateTime.UtcNow);
        var result = interview.MarkResult(InterviewResultCode.Failed, "user1", DateTime.UtcNow);
        result.Should().BeFalse();
    }

    [Fact]
    public void Feedback_Create_Valid_ShouldSucceed()
    {
        var feedback = InterviewFeedback.Create(
            new InterviewFeedbackId(Guid.NewGuid()),
            new InterviewScheduleId(Guid.NewGuid()),
            new EmployeeId(Guid.NewGuid()),
            4,
            "Strong technical skills",
            "Needs more domain knowledge",
            InterviewRecommendationCode.Hire,
            DateTime.UtcNow);
        feedback.Should().NotBeNull();
        feedback.Rating.Should().Be(4);
    }

    [Fact]
    public void Feedback_RatingBelow1_ShouldFail()
    {
        var feedback = InterviewFeedback.Create(
            new InterviewFeedbackId(Guid.NewGuid()),
            new InterviewScheduleId(Guid.NewGuid()),
            new EmployeeId(Guid.NewGuid()),
            0,
            "Strengths",
            "Concerns",
            InterviewRecommendationCode.Hire,
            DateTime.UtcNow);
        feedback.Should().BeNull();
    }

    [Fact]
    public void Feedback_RatingAbove5_ShouldFail()
    {
        var feedback = InterviewFeedback.Create(
            new InterviewFeedbackId(Guid.NewGuid()),
            new InterviewScheduleId(Guid.NewGuid()),
            new EmployeeId(Guid.NewGuid()),
            6,
            "Strengths",
            "Concerns",
            InterviewRecommendationCode.Hire,
            DateTime.UtcNow);
        feedback.Should().BeNull();
    }

    [Fact]
    public void Feedback_MissingRecommendation_ShouldFail()
    {
        var feedback = InterviewFeedback.Create(
            new InterviewFeedbackId(Guid.NewGuid()),
            new InterviewScheduleId(Guid.NewGuid()),
            new EmployeeId(Guid.NewGuid()),
            3,
            "Strengths",
            "Concerns",
            "",
            DateTime.UtcNow);
        feedback.Should().BeNull();
    }

    [Fact]
    public void Feedback_DuplicateInterviewer_EnforcedByUniqueIndex()
    {
        var scheduleId = new InterviewScheduleId(Guid.NewGuid());
        var interviewerId = new EmployeeId(Guid.NewGuid());

        var feedback1 = InterviewFeedback.Create(
            new InterviewFeedbackId(Guid.NewGuid()), scheduleId, interviewerId,
            4, "Good", "None", InterviewRecommendationCode.Hire, DateTime.UtcNow);
        feedback1.Should().NotBeNull();

        var feedback2 = InterviewFeedback.Create(
            new InterviewFeedbackId(Guid.NewGuid()), scheduleId, interviewerId,
            3, "Okay", "Some concerns", InterviewRecommendationCode.Neutral, DateTime.UtcNow);
        feedback2.Should().NotBeNull();
    }
}
