using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.Contract.Identity.ModelIds;
using Anemoi.Hr.Application.Abstractions;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Services;
using Anemoi.Hr.Domain.Workflow;
using Anemoi.Hr.ModelIds.ModelIds;
using Anemoi.Hr.Test.Helpers;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Anemoi.Hr.Test.Application.Workflow;

public sealed class WorkflowBuilderTests
{
    [Fact]
    public async Task BuildAsync_WithDefinitionFound_ReturnsStepsFromDefinition()
    {
        var defId = new WorkflowDefinitionId(Guid.NewGuid());
        var step1 = WorkflowDefinitionStep.Create(
            new WorkflowDefinitionStepId(Guid.NewGuid()), defId, 1,
            ApproverType.Role, "HR_Manager", true);
        var step2 = WorkflowDefinitionStep.Create(
            new WorkflowDefinitionStepId(Guid.NewGuid()), defId, 2,
            ApproverType.Role, "Director", true);

        var definition = WorkflowDefinition.Create(defId, "REQ-001", "Test", null,
            WorkflowTypeCode.Approval, "SomeEntity", 1, [step1, step2]);
        definition.Activate();

        var definitionRepo = Substitute.For<ISqlRepository<WorkflowDefinition>>();
        definitionRepo.GetQueryable().Returns(
            AsyncQueryableHelper.CreateMockQueryable(new[] { definition }));

        var hierarchyResolver = Substitute.For<IWorkflowHierarchyResolver>();
        var builder = new WorkflowBuilder(definitionRepo, hierarchyResolver);

        var result = await builder.BuildAsync(
            "SomeEntity", new EmployeeId(Guid.NewGuid()), "user-1", CancellationToken.None);

        result.IsT0.Should().BeTrue();
        var buildResult = result.AsT0;
        buildResult.Steps.Should().HaveCount(2);
        buildResult.Steps.ElementAt(0).Sequence.Should().Be(1);
        buildResult.Steps.ElementAt(0).ApproverTypeSnapshot.Should().Be(ApproverType.Role);
        buildResult.Steps.ElementAt(0).ApproverValueSnapshot.Should().Be("HR_Manager");
        buildResult.Steps.ElementAt(1).Sequence.Should().Be(2);
        buildResult.Steps.ElementAt(1).ApproverValueSnapshot.Should().Be("Director");
        buildResult.DefinitionId.Should().NotBeNull();
        buildResult.DefinitionName.Should().Be("Test");
        buildResult.DefinitionVersion.Should().Be(1);
    }

    [Fact]
    public async Task BuildAsync_NoDefinitionForLeaveRequest_ReturnsError()
    {
        var definitionRepo = Substitute.For<ISqlRepository<WorkflowDefinition>>();
        definitionRepo.GetQueryable().Returns(
            AsyncQueryableHelper.CreateMockQueryable<WorkflowDefinition>([]));

        var hierarchyResolver = Substitute.For<IWorkflowHierarchyResolver>();
        var builder = new WorkflowBuilder(definitionRepo, hierarchyResolver);

        var result = await builder.BuildAsync(
            WorkflowConstants.TargetEntityTypes.LeaveRequest,
            new EmployeeId(Guid.NewGuid()), "user-1", CancellationToken.None);

        result.IsT1.Should().BeTrue();
        result.AsT1.ErrorCode.Should().Be("HR_WF_DEF_REQUIRES_DEFINITION");
    }

    [Fact]
    public async Task BuildAsync_NoDefinitionForPayrollRun_ReturnsError()
    {
        var definitionRepo = Substitute.For<ISqlRepository<WorkflowDefinition>>();
        definitionRepo.GetQueryable().Returns(
            AsyncQueryableHelper.CreateMockQueryable<WorkflowDefinition>([]));

        var hierarchyResolver = Substitute.For<IWorkflowHierarchyResolver>();
        var builder = new WorkflowBuilder(definitionRepo, hierarchyResolver);

        var result = await builder.BuildAsync(
            WorkflowConstants.TargetEntityTypes.PayrollRun,
            new EmployeeId(Guid.NewGuid()), "user-1", CancellationToken.None);

        result.IsT1.Should().BeTrue();
        result.AsT1.ErrorCode.Should().Be("HR_WF_DEF_REQUIRES_DEFINITION");
    }
}
