using Anemoi.Hr.Domain.Workflow;
using Anemoi.Hr.ModelIds.ModelIds;
using FluentAssertions;
using Xunit;

namespace Anemoi.Hr.Test.Domain.Workflow;

public sealed class WorkflowDefinitionTests
{
    private static (WorkflowDefinition Definition, WorkflowDefinitionId Id) CreateDefinitionWithStep()
    {
        var id = new WorkflowDefinitionId(Guid.NewGuid());
        var step = WorkflowDefinitionStep.Create(
            new WorkflowDefinitionStepId(Guid.NewGuid()), id, 1,
            ApproverType.Role, "HR_Manager", true);
        var def = WorkflowDefinition.Create(id, "REQ-001", "Test Workflow", null,
            WorkflowTypeCode.Approval, [step]);
        return (def, id);
    }

    [Fact]
    public void Create_ShouldSetProperties()
    {
        var (def, id) = CreateDefinitionWithStep();

        def.Id.Should().Be(id);
        def.Code.Should().Be("REQ-001");
        def.Name.Should().Be("Test Workflow");
        def.Description.Should().BeNull();
        def.WorkflowTypeCode.Should().Be(WorkflowTypeCode.Approval);
        def.IsActive.Should().BeFalse();
        def.Steps.Should().HaveCount(1);
    }

    [Fact]
    public void Activate_WithSteps_ShouldSetActive()
    {
        var (def, _) = CreateDefinitionWithStep();

        def.Activate();

        def.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Activate_WithoutSteps_ShouldThrow()
    {
        var id = new WorkflowDefinitionId(Guid.NewGuid());
        var def = WorkflowDefinition.Create(id, "TST", "Test", null,
            WorkflowTypeCode.Approval, []);

        Action act = () => def.Activate();
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Deactivate_ShouldSetInactive()
    {
        var (def, _) = CreateDefinitionWithStep();
        def.Activate();

        def.Deactivate();

        def.IsActive.Should().BeFalse();
    }

    [Fact]
    public void ReplaceSteps_WhenActive_ShouldThrow()
    {
        var (def, _) = CreateDefinitionWithStep();
        def.Activate();

        Action act = () => def.ReplaceSteps([]);
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void ReplaceSteps_WhenInactive_ShouldSucceed()
    {
        var (def, id) = CreateDefinitionWithStep();
        var newStep = WorkflowDefinitionStep.Create(
            new WorkflowDefinitionStepId(Guid.NewGuid()), id, 1,
            ApproverType.SpecificUser, "user-1", false);

        def.ReplaceSteps([newStep]);

        def.Steps.Should().HaveCount(1);
        def.Steps.First().ApproverType.Should().Be(ApproverType.SpecificUser);
    }

    [Fact]
    public void UpdateDetails_ShouldUpdateNameAndDescription()
    {
        var (def, _) = CreateDefinitionWithStep();

        def.UpdateDetails("New Name", "New Description");

        def.Name.Should().Be("New Name");
        def.Description.Should().Be("New Description");
    }

    [Fact]
    public void Steps_ShouldBeInCreationOrder()
    {
        var id = new WorkflowDefinitionId(Guid.NewGuid());
        var step1 = WorkflowDefinitionStep.Create(
            new WorkflowDefinitionStepId(Guid.NewGuid()), id, 1,
            ApproverType.Role, "Manager", true);
        var step2 = WorkflowDefinitionStep.Create(
            new WorkflowDefinitionStepId(Guid.NewGuid()), id, 2,
            ApproverType.Role, "Director", true);
        var def = WorkflowDefinition.Create(id, "TST", "Test", null,
            WorkflowTypeCode.Approval, [step1, step2]);

        def.Steps.Should().HaveCount(2);
        def.Steps.ElementAt(0).Sequence.Should().Be(1);
        def.Steps.ElementAt(1).Sequence.Should().Be(2);
    }
}
