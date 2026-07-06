using Anemoi.Hr.Domain.Onboarding;
using Anemoi.Hr.ModelIds.ModelIds;
using FluentAssertions;
using Xunit;

namespace Anemoi.Hr.Test.Domain.Onboarding;

public class OnboardingTaskTests
{
    private static OnboardingTask CreatePendingTask()
    {
        return OnboardingTask.Create(
            new OnboardingTaskId(Guid.NewGuid()),
            "Setup workstation",
            "Install required software",
            AssigneeRoleCode.It,
            "user2",
            "John Doe",
            "user1",
            DateTime.UtcNow.AddDays(7),
            1);
    }

    [Fact]
    public void Create_ShouldSetPendingStatus()
    {
        var task = CreatePendingTask();
        task.Status.Should().Be(OnboardingTaskStatusCode.Pending);
    }

    [Fact]
    public void Create_ShouldSetAssignedAt()
    {
        var task = CreatePendingTask();
        task.AssignedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Complete_WhenPending_ShouldSetCompleted()
    {
        var task = CreatePendingTask();
        var result = task.Complete("user2");
        result.Should().BeTrue();
        task.Status.Should().Be(OnboardingTaskStatusCode.Completed);
    }

    [Fact]
    public void Complete_WhenAlreadyCompleted_ShouldReturnFalse()
    {
        var task = CreatePendingTask();
        task.Complete("user2");
        var result = task.Complete("user3");
        result.Should().BeFalse();
    }

    [Fact]
    public void Complete_ShouldSetCompletedByAndAt()
    {
        var task = CreatePendingTask();
        task.Complete("user2", "All done");
        task.CompletedBy.Should().Be("user2");
        task.CompletedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        task.CompletedNotes.Should().Be("All done");
    }

    [Fact]
    public void Skip_WhenPending_ShouldSetSkipped()
    {
        var task = CreatePendingTask();
        var result = task.Skip("user2");
        result.Should().BeTrue();
        task.Status.Should().Be(OnboardingTaskStatusCode.Skipped);
    }

    [Fact]
    public void Skip_WhenAlreadySkipped_ShouldReturnFalse()
    {
        var task = CreatePendingTask();
        task.Skip("user2");
        var result = task.Skip("user3");
        result.Should().BeFalse();
    }

    [Fact]
    public void Reopen_WhenCompleted_ShouldSetPending()
    {
        var task = CreatePendingTask();
        task.Complete("user2");
        var result = task.Reopen("user3", "Needs rework");
        result.Should().BeTrue();
        task.Status.Should().Be(OnboardingTaskStatusCode.Pending);
    }

    [Fact]
    public void Reopen_WhenSkipped_ShouldSetPending()
    {
        var task = CreatePendingTask();
        task.Skip("user2");
        var result = task.Reopen("user3");
        result.Should().BeTrue();
        task.Status.Should().Be(OnboardingTaskStatusCode.Pending);
    }

    [Fact]
    public void Reopen_WhenAlreadyPending_ShouldReturnFalse()
    {
        var task = CreatePendingTask();
        var result = task.Reopen("user2");
        result.Should().BeFalse();
    }

    [Fact]
    public void Reopen_ShouldSetReopenedByAndAtAndReason()
    {
        var task = CreatePendingTask();
        task.Complete("user2");
        task.Reopen("user3", "Needs rework");
        task.ReopenedBy.Should().Be("user3");
        task.ReopenedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        task.ReopenedReason.Should().Be("Needs rework");
    }

    [Fact]
    public void Reassign_WhenPending_ShouldUpdateAssignedUserId()
    {
        var task = CreatePendingTask();
        var result = task.Reassign("user4", "Jane Doe", "user1");
        result.Should().BeTrue();
        task.AssignedUserId.Should().Be("user4");
        task.AssignedUserDisplayNameSnapshot.Should().Be("Jane Doe");
    }

    [Fact]
    public void Reassign_WhenCompleted_ShouldReturnFalse()
    {
        var task = CreatePendingTask();
        task.Complete("user2");
        var result = task.Reassign("user3", "New User", "user1");
        result.Should().BeFalse();
    }

    [Fact]
    public void IsOverdue_WhenPendingAndPastDue_ShouldReturnTrue()
    {
        var task = OnboardingTask.Create(
            new OnboardingTaskId(Guid.NewGuid()),
            "Overdue task", null, AssigneeRoleCode.Hr, "user2",
            "John", "user1", DateTime.UtcNow.AddDays(-1), 1);
        task.IsOverdue().Should().BeTrue();
    }

    [Fact]
    public void IsOverdue_WhenCompleted_ShouldReturnFalse()
    {
        var task = OnboardingTask.Create(
            new OnboardingTaskId(Guid.NewGuid()),
            "Completed task", null, AssigneeRoleCode.Hr, "user2",
            "John", "user1", DateTime.UtcNow.AddDays(-1), 1);
        task.Complete("user2");
        task.IsOverdue().Should().BeFalse();
    }

    [Fact]
    public void IsOverdue_WhenPendingAndFutureDue_ShouldReturnFalse()
    {
        var task = CreatePendingTask();
        task.IsOverdue().Should().BeFalse();
    }
}
