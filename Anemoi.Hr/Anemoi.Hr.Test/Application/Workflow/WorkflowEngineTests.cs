using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Contract.Identity.ModelIds;
using Anemoi.Hr.Application.Abstractions;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Services;
using Anemoi.Hr.Domain.Workflow;
using Anemoi.Hr.ModelIds.ModelIds;
using Anemoi.Hr.Test.Helpers;
using FluentAssertions;
using NSubstitute;
using OneOf;
using Xunit;

namespace Anemoi.Hr.Test.Application.Workflow;

public sealed class WorkflowEngineTests
{
    [Fact]
    public async Task StartAsync_WithValidSteps_CreatesInstanceAndReturnsIt()
    {
        var workflowBuilder = Substitute.For<IWorkflowBuilder>();
        var stepId = new WorkflowInstanceStepId(Guid.NewGuid());
        var steps = new List<WorkflowInstanceStep>
        {
            WorkflowInstanceStep.Create(stepId, default, 1,
                ApproverType.SpecificUser, null, "user-1")
        };
        var buildResult = OneOf<IReadOnlyList<WorkflowInstanceStep>, WorkflowBuildError>
            .FromT0(steps);
        workflowBuilder.BuildAsync(Arg.Any<string>(), Arg.Any<EmployeeId>(),
                Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(buildResult);

        var instanceRepo = Substitute.For<ISqlRepository<WorkflowInstance>>();
        instanceRepo.CreateOneAsync(Arg.Any<WorkflowInstance>(), Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                var instance = callInfo.Arg<WorkflowInstance>();
                return Task.FromResult<OneOf<WorkflowInstance, Exception>>(instance);
            });

        var historyRepo = Substitute.For<ISqlRepository<WorkflowHistory>>();
        var engine = new WorkflowEngine(instanceRepo, historyRepo, workflowBuilder);

        var result = await engine.StartAsync(
            "TestEntity", Guid.NewGuid(),
            new EmployeeId(Guid.NewGuid()), new UserId(Guid.NewGuid()),
            new UserId(Guid.NewGuid()), CancellationToken.None);

        result.IsT0.Should().BeTrue();
        var instance = result.AsT0;
        instance.Status.Should().Be(WorkflowStatusCode.Pending);
        instance.EntityType.Should().Be("TestEntity");
        instance.Steps.Should().HaveCount(1);

        await instanceRepo.Received(1).CreateOneAsync(
            Arg.Any<WorkflowInstance>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ApproveAsync_NonExistentInstance_ReturnsError()
    {
        var workflowBuilder = Substitute.For<IWorkflowBuilder>();
        var instanceRepo = Substitute.For<ISqlRepository<WorkflowInstance>>();
        instanceRepo.GetQueryable().Returns(
            AsyncQueryableHelper.CreateMockQueryable<WorkflowInstance>([]));

        var historyRepo = Substitute.For<ISqlRepository<WorkflowHistory>>();
        var engine = new WorkflowEngine(instanceRepo, historyRepo, workflowBuilder);

        var result = await engine.ApproveAsync(
            new WorkflowInstanceId(Guid.NewGuid()),
            new UserId(Guid.NewGuid()), null, CancellationToken.None);

        result.IsT1.Should().BeTrue();
        result.AsT1.Code.Should().Be(HrBusinessErrorCodes.WorkflowInstanceNotFound);
    }
}
