using Anemoi.Hr.Domain.Onboarding;
using Anemoi.Hr.ModelIds.ModelIds;
using FluentAssertions;
using Xunit;

namespace Anemoi.Hr.Test.Domain.Onboarding;

public class OnboardingInstanceTests
{
    private static readonly EmployeeId EmployeeId = new(Guid.NewGuid());
    private static readonly OnboardingPlanTemplateId TemplateId = new(Guid.NewGuid());

    private static OnboardingInstance CreateInstance()
    {
        return OnboardingInstance.Create(
            new OnboardingInstanceId(Guid.NewGuid()),
            EmployeeId,
            TemplateId,
            "Standard Onboarding",
            1,
            DateTime.UtcNow,
            "user1");
    }

    private static OnboardingTask CreateTask(int sortOrder = 1, int daysFromNow = 7)
    {
        return OnboardingTask.Create(
            new OnboardingTaskId(Guid.NewGuid()),
            $"Task {sortOrder}", null, AssigneeRoleCode.Hr,
            "user2", "John Doe", "user1",
            DateTime.UtcNow.AddDays(daysFromNow), sortOrder);
    }

    [Fact]
    public void Create_ShouldSetInProgressStatus()
    {
        var instance = CreateInstance();
        instance.Status.Should().Be(OnboardingInstanceStatusCode.InProgress);
    }

    [Fact]
    public void Create_ShouldSetTemplateIdAndTemplateVersion()
    {
        var instance = CreateInstance();
        instance.TemplateId.Should().Be(TemplateId);
        instance.TemplateVersion.Should().Be(1);
    }

    [Fact]
    public void AddTask_ShouldAddToTasksCollection()
    {
        var instance = CreateInstance();
        var task = CreateTask();
        instance.AddTask(task);
        instance.Tasks.Should().ContainSingle();
    }

    [Fact]
    public void CompleteTask_WhenValid_ShouldCompleteTask()
    {
        var instance = CreateInstance();
        var task = CreateTask();
        instance.AddTask(task);

        var result = instance.CompleteTask(task.Id, "user2");
        result.Should().BeTrue();
        task.Status.Should().Be(OnboardingTaskStatusCode.Completed);
    }

    [Fact]
    public void CompleteTask_WhenAllTasksDone_ShouldAutoCompleteInstance()
    {
        var instance = CreateInstance();
        var task1 = CreateTask(1);
        var task2 = CreateTask(2);
        instance.AddTask(task1);
        instance.AddTask(task2);

        instance.CompleteTask(task1.Id, "user2");
        instance.Status.Should().Be(OnboardingInstanceStatusCode.InProgress);

        instance.CompleteTask(task2.Id, "user2");
        instance.Status.Should().Be(OnboardingInstanceStatusCode.Completed);
        instance.CompletedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void CompleteTask_WhenTaskNotFound_ShouldReturnFalse()
    {
        var instance = CreateInstance();
        var result = instance.CompleteTask(new OnboardingTaskId(Guid.NewGuid()), "user2");
        result.Should().BeFalse();
    }

    [Fact]
    public void SkipTask_WhenValid_ShouldSkipTask()
    {
        var instance = CreateInstance();
        var task = CreateTask();
        instance.AddTask(task);

        var result = instance.SkipTask(task.Id, "user2");
        result.Should().BeTrue();
        task.Status.Should().Be(OnboardingTaskStatusCode.Skipped);
    }

    [Fact]
    public void SkipTask_WhenAllTasksDone_ShouldAutoCompleteInstance()
    {
        var instance = CreateInstance();
        var task = CreateTask();
        instance.AddTask(task);

        instance.SkipTask(task.Id, "user2");
        instance.Status.Should().Be(OnboardingInstanceStatusCode.Completed);
    }

    [Fact]
    public void ReopenTask_WhenCompleted_ShouldReopenTaskAndRevertInstanceToInProgress()
    {
        var instance = CreateInstance();
        var task = CreateTask();
        instance.AddTask(task);
        instance.CompleteTask(task.Id, "user2");
        instance.Status.Should().Be(OnboardingInstanceStatusCode.Completed);

        var result = instance.ReopenTask(task.Id, "user3", "Rework");
        result.Should().BeTrue();
        task.Status.Should().Be(OnboardingTaskStatusCode.Pending);
        instance.Status.Should().Be(OnboardingInstanceStatusCode.InProgress);
    }

    [Fact]
    public void ReopenTask_WhenAutoCompletedInstance_ShouldRevertToInProgress()
    {
        var instance = CreateInstance();
        var task = CreateTask();
        instance.AddTask(task);
        instance.CompleteTask(task.Id, "user2");
        instance.Status.Should().Be(OnboardingInstanceStatusCode.Completed);

        instance.ReopenTask(task.Id, "user3");
        instance.Status.Should().Be(OnboardingInstanceStatusCode.InProgress);
        instance.CompletedAt.Should().BeNull();
        instance.CompletedBy.Should().BeNull();
    }

    [Fact]
    public void Cancel_WhenInProgress_ShouldSetCancelled()
    {
        var instance = CreateInstance();
        var result = instance.Cancel("user1");
        result.Should().BeTrue();
        instance.Status.Should().Be(OnboardingInstanceStatusCode.Cancelled);
        instance.CancelledBy.Should().Be("user1");
        instance.CancelledAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Cancel_WhenCompleted_ShouldReturnFalse()
    {
        var instance = CreateInstance();
        var task = CreateTask();
        instance.AddTask(task);
        instance.CompleteTask(task.Id, "user2");
        var result = instance.Cancel("user1");
        result.Should().BeFalse();
    }

    [Fact]
    public void Cancel_WhenCancelled_ShouldReturnFalse()
    {
        var instance = CreateInstance();
        instance.Cancel("user1");
        var result = instance.Cancel("user2");
        result.Should().BeFalse();
    }

    [Fact]
    public void Reopen_WhenCompleted_ShouldResetToInProgress()
    {
        var instance = CreateInstance();
        var task = CreateTask();
        instance.AddTask(task);
        instance.CompleteTask(task.Id, "user2");

        var result = instance.Reopen("user3");
        result.Should().BeTrue();
        instance.Status.Should().Be(OnboardingInstanceStatusCode.InProgress);
    }

    [Fact]
    public void Reopen_WhenCancelled_ShouldResetToInProgress()
    {
        var instance = CreateInstance();
        instance.Cancel("user1");

        var result = instance.Reopen("user2");
        result.Should().BeTrue();
        instance.Status.Should().Be(OnboardingInstanceStatusCode.InProgress);
    }

    [Fact]
    public void Reopen_ShouldClearCompletedAndCancelledAuditFields()
    {
        var instance = CreateInstance();
        var task = CreateTask();
        instance.AddTask(task);
        instance.CompleteTask(task.Id, "user2");

        instance.Reopen("user3");
        instance.CompletedAt.Should().BeNull();
        instance.CompletedBy.Should().BeNull();
        instance.CancelledAt.Should().BeNull();
        instance.CancelledBy.Should().BeNull();
        instance.ForceCompletedAt.Should().BeNull();
        instance.ForceCompletedBy.Should().BeNull();
        instance.ForceCompleteReason.Should().BeNull();
    }

    [Fact]
    public void ForceComplete_WhenInProgress_ShouldSetCompletedWithForceFields()
    {
        var instance = CreateInstance();
        var result = instance.ForceComplete("manager1", "Employee resigned");
        result.Should().BeTrue();
        instance.Status.Should().Be(OnboardingInstanceStatusCode.Completed);
        instance.ForceCompletedBy.Should().Be("manager1");
        instance.ForceCompleteReason.Should().Be("Employee resigned");
        instance.ForceCompletedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        instance.CompletedAt.Should().Be(instance.ForceCompletedAt);
        instance.CompletedBy.Should().Be("manager1");
    }

    [Fact]
    public void ForceComplete_WhenCompleted_ShouldReturnFalse()
    {
        var instance = CreateInstance();
        instance.ForceComplete("manager1", "Reason");
        var result = instance.ForceComplete("manager2", "Another reason");
        result.Should().BeFalse();
    }

    [Fact]
    public void GetCompletionPercentage_WhenNoTasks_ShouldReturnZero()
    {
        var instance = CreateInstance();
        instance.GetCompletionPercentage().Should().Be(0);
    }

    [Fact]
    public void GetCompletionPercentage_WhenHalfDone_ShouldReturnFifty()
    {
        var instance = CreateInstance();
        var task1 = CreateTask(1);
        var task2 = CreateTask(2);
        instance.AddTask(task1);
        instance.AddTask(task2);

        instance.CompleteTask(task1.Id, "user2");
        instance.GetCompletionPercentage().Should().Be(50);
    }
}
