using Anemoi.Hr.Domain.Workflow;
using Anemoi.Hr.ModelIds.ModelIds;
using FluentAssertions;
using Xunit;

namespace Anemoi.Hr.Test.Domain.Workflow;

public sealed class WorkflowInstanceTests
{
    private static (WorkflowInstance Instance, WorkflowInstanceId Id) CreatePendingInstance(int stepCount = 1)
    {
        var defId = new WorkflowDefinitionId(Guid.NewGuid());
        var instanceId = new WorkflowInstanceId(Guid.NewGuid());
        var steps = Enumerable.Range(1, stepCount).Select(i =>
            WorkflowInstanceStep.Create(
                new WorkflowInstanceStepId(Guid.NewGuid()), instanceId, i,
                ApproverType.SpecificUser, "user-1", "user-1")).ToList();

        var instance = WorkflowInstance.Start(instanceId, defId,
            "TestEntity", "entity-1", "requester-1", steps);
        return (instance, instanceId);
    }

    [Fact]
    public void Start_ShouldSetPending()
    {
        var (instance, _) = CreatePendingInstance();

        instance.Status.Should().Be(WorkflowStatusCode.Pending);
        instance.CurrentStep.Should().Be(1);
        instance.StartedBy.Should().Be("requester-1");
        instance.EntityType.Should().Be("TestEntity");
        instance.EntityId.Should().Be("entity-1");
    }

    [Fact]
    public void Start_WithoutSteps_ShouldHaveCurrentStepZero()
    {
        var defId = new WorkflowDefinitionId(Guid.NewGuid());
        var instanceId = new WorkflowInstanceId(Guid.NewGuid());
        var instance = WorkflowInstance.Start(instanceId, defId,
            "Test", "e-1", "user", []);

        instance.CurrentStep.Should().Be(0);
    }

    [Fact]
    public void Approve_LastStep_ShouldComplete()
    {
        var (instance, _) = CreatePendingInstance(1);

        instance.Approve("user-1", null);

        instance.Status.Should().Be(WorkflowStatusCode.Approved);
        instance.CurrentStep.Should().Be(1);
        instance.CompletedAt.Should().NotBeNull();
        instance.Steps.First().Status.Should().Be(WorkflowStepStatusCode.Approved);
    }

    [Fact]
    public void Approve_FirstOfMultipleSteps_ShouldAdvance()
    {
        var (instance, _) = CreatePendingInstance(3);

        instance.Approve("user-1", null);

        instance.Status.Should().Be(WorkflowStatusCode.Pending);
        instance.CurrentStep.Should().Be(2);
    }

    [Fact]
    public void Approve_AllSteps_ShouldComplete()
    {
        var (instance, _) = CreatePendingInstance(3);

        instance.Approve("user-1", null);
        instance.Approve("user-1", null);
        instance.Approve("user-1", null);

        instance.Status.Should().Be(WorkflowStatusCode.Approved);
        instance.CompletedAt.Should().NotBeNull();
        instance.Steps.Should().AllSatisfy(s => s.Status.Should().Be(WorkflowStepStatusCode.Approved));
    }

    [Fact]
    public void Reject_ShouldSetRejected()
    {
        var (instance, _) = CreatePendingInstance(3);

        instance.Reject("user-1", "Not approved");

        instance.Status.Should().Be(WorkflowStatusCode.Rejected);
        instance.CompletedAt.Should().NotBeNull();
        instance.Steps.First().Status.Should().Be(WorkflowStepStatusCode.Rejected);
    }

    [Fact]
    public void Reject_ShouldCreateHistory()
    {
        var (instance, _) = CreatePendingInstance(1);

        var history = instance.Reject("user-1", "Not good enough");

        history.Should().NotBeNull();
        history.Action.Should().Be("Reject");
        history.PerformedBy.Should().Be("user-1");
        history.Comment.Should().Be("Not good enough");
    }

    [Fact]
    public void Cancel_ShouldSetCancelled()
    {
        var (instance, _) = CreatePendingInstance(2);

        instance.Cancel("requester-1");

        instance.Status.Should().Be(WorkflowStatusCode.Cancelled);
        instance.CompletedAt.Should().NotBeNull();
        instance.Steps.Should().AllSatisfy(s => s.Status.Should().Be(WorkflowStepStatusCode.Cancelled));
    }

    [Fact]
    public void Cancel_AlreadyApproved_ShouldThrow()
    {
        var (instance, _) = CreatePendingInstance(1);
        instance.Approve("user-1", null);

        Action act = () => instance.Cancel("requester-1");
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Cancel_AlreadyRejected_ShouldThrow()
    {
        var (instance, _) = CreatePendingInstance(1);
        instance.Reject("user-1", null);

        Action act = () => instance.Cancel("requester-1");
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void ReturnForRevision_ShouldSetReturned()
    {
        var (instance, _) = CreatePendingInstance(2);

        instance.ReturnForRevision("user-1", "Please revise");

        instance.Status.Should().Be(WorkflowStatusCode.Returned);
        instance.CompletedAt.Should().NotBeNull();
    }

    [Fact]
    public void ReturnForRevision_ShouldCancelPendingSteps()
    {
        var (instance, _) = CreatePendingInstance(3);

        instance.ReturnForRevision("user-1", "Please revise");

        instance.Steps.Should().AllSatisfy(s => s.Status.Should().Be(WorkflowStepStatusCode.Cancelled));
    }

    [Fact]
    public void Approve_ShouldCreateHistory()
    {
        var (instance, _) = CreatePendingInstance(1);

        var history = instance.Approve("user-1", "Looks good");

        history.Should().NotBeNull();
        history.Action.Should().Be("Approve");
        history.PerformedBy.Should().Be("user-1");
        history.Comment.Should().Be("Looks good");
    }

    [Fact]
    public void Approve_NotCurrentStepApprover_ShouldFail()
    {
        var (instance, _) = CreatePendingInstance(1);

        var result = instance.IsCurrentStepApprover("wrong-user",
            _ => false, _ => false);

        result.Should().BeFalse();
    }

    [Fact]
    public void Approve_CurrentSpecificUser_ShouldPass()
    {
        var (instance, _) = CreatePendingInstance(1);

        var result = instance.IsCurrentStepApprover("user-1",
            _ => false, _ => false);

        result.Should().BeTrue();
    }

    [Fact]
    public void Approve_WithRole_ShouldPass()
    {
        var defId = new WorkflowDefinitionId(Guid.NewGuid());
        var instanceId = new WorkflowInstanceId(Guid.NewGuid());
        var step = WorkflowInstanceStep.Create(
            new WorkflowInstanceStepId(Guid.NewGuid()), instanceId, 1,
            ApproverType.Role, "HR_Manager", null);
        var instance = WorkflowInstance.Start(instanceId, defId,
            "Test", "e-1", "requester-1", [step]);

        var result = instance.IsCurrentStepApprover("any-user",
            role => role == "HR_Manager", _ => false);

        result.Should().BeTrue();
    }

    [Fact]
    public void Approve_WithPermission_ShouldPass()
    {
        var defId = new WorkflowDefinitionId(Guid.NewGuid());
        var instanceId = new WorkflowInstanceId(Guid.NewGuid());
        var step = WorkflowInstanceStep.Create(
            new WorkflowInstanceStepId(Guid.NewGuid()), instanceId, 1,
            ApproverType.Permission, "hr.workflow.execute", null);
        var instance = WorkflowInstance.Start(instanceId, defId,
            "Test", "e-1", "requester-1", [step]);

        var result = instance.IsCurrentStepApprover("any-user",
            _ => false, perm => perm == "hr.workflow.execute");

        result.Should().BeTrue();
    }

    [Fact]
    public void Approve_ByDirectManager_ShouldPass()
    {
        var defId = new WorkflowDefinitionId(Guid.NewGuid());
        var instanceId = new WorkflowInstanceId(Guid.NewGuid());
        var step = WorkflowInstanceStep.Create(
            new WorkflowInstanceStepId(Guid.NewGuid()), instanceId, 1,
            ApproverType.DirectManager, null, "manager-1");
        var instance = WorkflowInstance.Start(instanceId, defId,
            "Test", "e-1", "requester-1", [step]);

        var result = instance.IsCurrentStepApprover("manager-1",
            _ => false, _ => false);

        result.Should().BeTrue();
    }

    [Fact]
    public void Approve_OnCompletedInstance_ShouldThrow()
    {
        var (instance, _) = CreatePendingInstance(1);
        instance.Approve("user-1", null);

        Action act = () => instance.Approve("user-1", null);
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Reject_OnCompletedInstance_ShouldThrow()
    {
        var (instance, _) = CreatePendingInstance(1);
        instance.Approve("user-1", null);

        Action act = () => instance.Reject("user-1", null);
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Instance_ShouldHaveHistories()
    {
        var (instance, _) = CreatePendingInstance(2);

        instance.Approve("user-1", "Step 1 ok");
        instance.Approve("user-1", "Step 2 ok");

        instance.Histories.Should().HaveCount(2);
        instance.Histories.ElementAt(0).Action.Should().Be("Approve");
        instance.Histories.ElementAt(1).Action.Should().Be("Approve");
    }

    [Fact]
    public void Start_ShouldInitializeStepsAsPending()
    {
        var (instance, _) = CreatePendingInstance(3);

        instance.Steps.Should().AllSatisfy(s =>
            s.Status.Should().Be(WorkflowStepStatusCode.Pending));
    }

    [Fact]
    public void Approve_ShouldStoreComment()
    {
        var (instance, _) = CreatePendingInstance(1);

        instance.Approve("user-1", "Approved with conditions");

        instance.Steps.First().Comment.Should().Be("Approved with conditions");
    }

    [Fact]
    public void Reject_ShouldStoreComment()
    {
        var (instance, _) = CreatePendingInstance(1);

        instance.Reject("user-1", "Missing documentation");

        instance.Steps.First().Comment.Should().Be("Missing documentation");
        instance.Steps.First().RejectedAt.Should().NotBeNull();
    }

    [Fact]
    public void Approve_ShouldSetApprovedAt()
    {
        var (instance, _) = CreatePendingInstance(1);

        instance.Approve("user-1", null);

        instance.Steps.First().ApprovedAt.Should().NotBeNull();
    }
}
