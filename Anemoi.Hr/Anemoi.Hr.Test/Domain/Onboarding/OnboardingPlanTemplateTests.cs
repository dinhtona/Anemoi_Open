using Anemoi.Hr.Domain.Onboarding;
using Anemoi.Hr.ModelIds.ModelIds;
using FluentAssertions;
using Xunit;

namespace Anemoi.Hr.Test.Domain.Onboarding;

public class OnboardingPlanTemplateTests
{
    private static OnboardingPlanTemplate CreateTemplate()
    {
        return OnboardingPlanTemplate.Create(
            new OnboardingPlanTemplateId(Guid.NewGuid()),
            "Standard Onboarding",
            "Standard onboarding plan for new employees",
            "user1");
    }

    [Fact]
    public void Create_ShouldSetInitialValues()
    {
        var template = CreateTemplate();
        template.Id.Should().NotBeNull();
        template.Name.Should().Be("Standard Onboarding");
        template.Description.Should().Be("Standard onboarding plan for new employees");
        template.Status.Should().Be(OnboardingPlanTemplateStatusCode.Active);
        template.Version.Should().Be(1);
    }

    [Fact]
    public void Create_WithNoTasks_ShouldHaveEmptyTaskTemplates()
    {
        var template = CreateTemplate();
        template.TaskTemplates.Should().BeEmpty();
    }

    [Fact]
    public void AddTaskTemplate_ShouldAddToCollection()
    {
        var template = CreateTemplate();
        var task = OnboardingTaskTemplate.Create(
            new OnboardingTaskTemplateId(Guid.NewGuid()),
            "Setup workstation", null, AssigneeRoleCode.It, 0, 1);
        template.AddTaskTemplate(task);
        template.TaskTemplates.Should().ContainSingle();
    }

    [Fact]
    public void Activate_WhenAlreadyActive_ShouldReturnFalse()
    {
        var template = CreateTemplate();
        var result = template.Activate();
        result.Should().BeFalse();
    }

    [Fact]
    public void Activate_WhenInactive_ShouldSetStatusToActive()
    {
        var template = CreateTemplate();
        template.Deactivate();
        var result = template.Activate();
        result.Should().BeTrue();
        template.Status.Should().Be(OnboardingPlanTemplateStatusCode.Active);
    }

    [Fact]
    public void Deactivate_WhenAlreadyInactive_ShouldReturnFalse()
    {
        var template = CreateTemplate();
        template.Deactivate();
        var result = template.Deactivate();
        result.Should().BeFalse();
    }

    [Fact]
    public void Deactivate_WhenActive_ShouldSetStatusToInactive()
    {
        var template = CreateTemplate();
        var result = template.Deactivate();
        result.Should().BeTrue();
        template.Status.Should().Be(OnboardingPlanTemplateStatusCode.Inactive);
    }

    [Fact]
    public void UpdateDetails_ShouldIncrementVersion()
    {
        var template = CreateTemplate();
        template.Version.Should().Be(1);
        var updatedTasks = new List<OnboardingTaskTemplate>
        {
            OnboardingTaskTemplate.Create(
                new OnboardingTaskTemplateId(Guid.NewGuid()),
                "New task", null, AssigneeRoleCode.Hr, 1, 1)
        };
        template.UpdateDetails("Updated Name", null, "user2", updatedTasks);
        template.Version.Should().Be(2);
    }

    [Fact]
    public void UpdateDetails_ShouldReplaceTaskTemplates()
    {
        var template = CreateTemplate();
        var initialTask = OnboardingTaskTemplate.Create(
            new OnboardingTaskTemplateId(Guid.NewGuid()),
            "Initial task", null, AssigneeRoleCode.It, 0, 1);
        template.AddTaskTemplate(initialTask);
        template.TaskTemplates.Should().HaveCount(1);

        var updatedTasks = new List<OnboardingTaskTemplate>();
        template.UpdateDetails("Updated", null, "user2", updatedTasks);
        template.TaskTemplates.Should().BeEmpty();
    }

    [Fact]
    public void UpdateDetails_ShouldUpdateNameAndDescription()
    {
        var template = CreateTemplate();
        var updatedTasks = new List<OnboardingTaskTemplate>();
        template.UpdateDetails("Updated Name", "Updated Description", "user2", updatedTasks);
        template.Name.Should().Be("Updated Name");
        template.Description.Should().Be("Updated Description");
    }
}
