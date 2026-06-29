using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Contract.Identity.ModelIds;
using Anemoi.Hr.Application.Abstractions;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Services;
using Anemoi.Hr.Domain.Employees;
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
        var buildResult = OneOf<WorkflowBuildResult, WorkflowBuildError>
            .FromT0(new WorkflowBuildResult(steps, null, null, null));
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
        var employeeRepo = Substitute.For<ISqlRepository<Employee>>();
        employeeRepo.GetQueryable().Returns(
            AsyncQueryableHelper.CreateMockQueryable<Employee>([]));

        var resolvedEmployeeId = new EmployeeId(Guid.NewGuid());
        var resolvedApprovers = new List<ResolvedApprover>
        {
            new(new UserId(Guid.NewGuid()), resolvedEmployeeId, "Test User", "test@test.com", "SpecificUser")
        };
        var approvalResolver = Substitute.For<IApprovalResolver>();
        approvalResolver.ResolveApproversAsync(Arg.Any<string>(), Arg.Any<string?>(),
                Arg.Any<ApprovalRoutingContext>(), Arg.Any<CancellationToken>())
            .Returns(OneOf<IReadOnlyList<ResolvedApprover>, ErrorDetailResponse>.FromT0(
                resolvedApprovers));

        var engine = new WorkflowEngine(instanceRepo, historyRepo, employeeRepo,
            workflowBuilder, approvalResolver);

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
    public async Task StartAsync_WithEmptySteps_ReturnsError()
    {
        var workflowBuilder = Substitute.For<IWorkflowBuilder>();
        var buildResult = OneOf<WorkflowBuildResult, WorkflowBuildError>
            .FromT0(new WorkflowBuildResult([], null, null, null));
        workflowBuilder.BuildAsync(Arg.Any<string>(), Arg.Any<EmployeeId>(),
                Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(buildResult);

        var instanceRepo = Substitute.For<ISqlRepository<WorkflowInstance>>();
        var historyRepo = Substitute.For<ISqlRepository<WorkflowHistory>>();
        var employeeRepo = Substitute.For<ISqlRepository<Employee>>();
        var approvalResolver = Substitute.For<IApprovalResolver>();
        var engine = new WorkflowEngine(instanceRepo, historyRepo, employeeRepo,
            workflowBuilder, approvalResolver);

        var result = await engine.StartAsync(
            "TestEntity", Guid.NewGuid(),
            new EmployeeId(Guid.NewGuid()), new UserId(Guid.NewGuid()),
            new UserId(Guid.NewGuid()), CancellationToken.None);

        result.IsT1.Should().BeTrue();
        result.AsT1.Code.Should().Be(HrBusinessErrorCodes.WorkflowApproverNotFound);

        await instanceRepo.DidNotReceive().CreateOneAsync(
            Arg.Any<WorkflowInstance>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task StartAsync_WhenActivationFails_ReturnsErrorAndDoesNotTrackInstance()
    {
        var workflowBuilder = Substitute.For<IWorkflowBuilder>();
        var stepId = new WorkflowInstanceStepId(Guid.NewGuid());
        var steps = new List<WorkflowInstanceStep>
        {
            WorkflowInstanceStep.Create(stepId, default, 1,
                ApproverType.DirectManager, null, null)
        };
        var buildResult = OneOf<WorkflowBuildResult, WorkflowBuildError>
            .FromT0(new WorkflowBuildResult(steps, null, null, null));
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
        var employeeRepo = Substitute.For<ISqlRepository<Employee>>();
        employeeRepo.GetQueryable().Returns(
            AsyncQueryableHelper.CreateMockQueryable<Employee>([]));

        var approvalResolver = Substitute.For<IApprovalResolver>();
        approvalResolver.ResolveApproversAsync(Arg.Any<string>(), Arg.Any<string?>(),
                Arg.Any<ApprovalRoutingContext>(), Arg.Any<CancellationToken>())
            .Returns(HrErrorResponses.Create(HrBusinessErrorCodes.WorkflowApproverNotFound));

        var engine = new WorkflowEngine(instanceRepo, historyRepo, employeeRepo,
            workflowBuilder, approvalResolver);

        var result = await engine.StartAsync(
            "TestEntity", Guid.NewGuid(),
            new EmployeeId(Guid.NewGuid()), new UserId(Guid.NewGuid()),
            new UserId(Guid.NewGuid()), CancellationToken.None);

        result.IsT1.Should().BeTrue();
        result.AsT1.Code.Should().Be(HrBusinessErrorCodes.WorkflowApproverNotFound);

        await instanceRepo.DidNotReceive().CreateOneAsync(
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
        var employeeRepo = Substitute.For<ISqlRepository<Employee>>();
        var approvalResolver = Substitute.For<IApprovalResolver>();
        var engine = new WorkflowEngine(instanceRepo, historyRepo, employeeRepo,
            workflowBuilder, approvalResolver);

        var result = await engine.ApproveAsync(
            new WorkflowInstanceId(Guid.NewGuid()),
            new UserId(Guid.NewGuid()), null, CancellationToken.None);

        result.IsT1.Should().BeTrue();
        result.AsT1.Code.Should().Be(HrBusinessErrorCodes.WorkflowInstanceNotFound);
    }

    [Fact]
    public async Task StartAsync_WithDefinitionResult_BindsDefinitionIdNameAndVersion()
    {
        var defId = new WorkflowDefinitionId(Guid.NewGuid());
        var defName = "Leave Request Approval";
        var defVersion = 2;
        var stepId = new WorkflowInstanceStepId(Guid.NewGuid());
        var steps = new List<WorkflowInstanceStep>
        {
            WorkflowInstanceStep.Create(stepId, default, 1,
                ApproverType.SpecificUser, null, "user-1")
        };

        var workflowBuilder = Substitute.For<IWorkflowBuilder>();
        var buildResult = OneOf<WorkflowBuildResult, WorkflowBuildError>
            .FromT0(new WorkflowBuildResult(steps, defId, defName, defVersion));
        workflowBuilder.BuildAsync(Arg.Any<string>(), Arg.Any<EmployeeId>(),
                Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(buildResult);

        WorkflowInstance? createdInstance = null;
        var instanceRepo = Substitute.For<ISqlRepository<WorkflowInstance>>();
        instanceRepo.CreateOneAsync(Arg.Any<WorkflowInstance>(), Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                createdInstance = callInfo.Arg<WorkflowInstance>();
                return Task.FromResult<OneOf<WorkflowInstance, Exception>>(createdInstance!);
            });

        var historyRepo = Substitute.For<ISqlRepository<WorkflowHistory>>();
        var employeeRepo = Substitute.For<ISqlRepository<Employee>>();
        employeeRepo.GetQueryable().Returns(
            AsyncQueryableHelper.CreateMockQueryable<Employee>([]));

        var resolvedEmployeeId = new EmployeeId(Guid.NewGuid());
        var resolvedApprovers = new List<ResolvedApprover>
        {
            new(new UserId(Guid.NewGuid()), resolvedEmployeeId, "Test User", "test@test.com", "SpecificUser")
        };
        var approvalResolver = Substitute.For<IApprovalResolver>();
        approvalResolver.ResolveApproversAsync(Arg.Any<string>(), Arg.Any<string?>(),
                Arg.Any<ApprovalRoutingContext>(), Arg.Any<CancellationToken>())
            .Returns(OneOf<IReadOnlyList<ResolvedApprover>, ErrorDetailResponse>.FromT0(
                resolvedApprovers));

        var engine = new WorkflowEngine(instanceRepo, historyRepo, employeeRepo,
            workflowBuilder, approvalResolver);

        var result = await engine.StartAsync(
            "LeaveRequest", Guid.NewGuid(),
            new EmployeeId(Guid.NewGuid()), new UserId(Guid.NewGuid()),
            new UserId(Guid.NewGuid()), CancellationToken.None);

        result.IsT0.Should().BeTrue();
        createdInstance.Should().NotBeNull();
        createdInstance!.WorkflowDefinitionId.Should().Be(defId);
        createdInstance.WorkflowDefinitionName.Should().Be(defName);
        createdInstance.WorkflowDefinitionVersion.Should().Be(defVersion);
    }

    [Fact]
    public async Task StartAsync_WithNullDefinitionForLeaveRequest_ReturnsError()
    {
        var stepId = new WorkflowInstanceStepId(Guid.NewGuid());
        var steps = new List<WorkflowInstanceStep>
        {
            WorkflowInstanceStep.Create(stepId, default, 1,
                ApproverType.DirectManager, null, null)
        };

        var workflowBuilder = Substitute.For<IWorkflowBuilder>();
        var buildResult = OneOf<WorkflowBuildResult, WorkflowBuildError>
            .FromT0(new WorkflowBuildResult(steps, null, null, null));
        workflowBuilder.BuildAsync(Arg.Any<string>(), Arg.Any<EmployeeId>(),
                Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(buildResult);

        var instanceRepo = Substitute.For<ISqlRepository<WorkflowInstance>>();
        var historyRepo = Substitute.For<ISqlRepository<WorkflowHistory>>();
        var employeeRepo = Substitute.For<ISqlRepository<Employee>>();
        var approvalResolver = Substitute.For<IApprovalResolver>();
        var engine = new WorkflowEngine(instanceRepo, historyRepo, employeeRepo,
            workflowBuilder, approvalResolver);

        var result = await engine.StartAsync(
            WorkflowConstants.TargetEntityTypes.LeaveRequest, Guid.NewGuid(),
            new EmployeeId(Guid.NewGuid()), new UserId(Guid.NewGuid()),
            new UserId(Guid.NewGuid()), CancellationToken.None);

        result.IsT1.Should().BeTrue();
        result.AsT1.Code.Should().Be(HrBusinessErrorCodes.WorkflowDefinitionRequiresDefinition);
    }

    [Theory]
    [InlineData("LeaveRequest")]
    [InlineData("OvertimeRequest")]
    [InlineData("PayrollRun")]
    [InlineData("RecruitmentRequest")]
    [InlineData("EmployeeTransfer")]
    [InlineData("EmployeeSeparation")]
    [InlineData("ProbationRecord")]
    public async Task StartAsync_RequiredTypeWithoutDefinition_ReturnsError(string entityType)
    {
        var stepId = new WorkflowInstanceStepId(Guid.NewGuid());
        var steps = new List<WorkflowInstanceStep>
        {
            WorkflowInstanceStep.Create(stepId, default, 1,
                ApproverType.DirectManager, null, null)
        };

        var workflowBuilder = Substitute.For<IWorkflowBuilder>();
        var buildResult = OneOf<WorkflowBuildResult, WorkflowBuildError>
            .FromT0(new WorkflowBuildResult(steps, null, null, null));
        workflowBuilder.BuildAsync(Arg.Any<string>(), Arg.Any<EmployeeId>(),
                Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(buildResult);

        var instanceRepo = Substitute.For<ISqlRepository<WorkflowInstance>>();
        var historyRepo = Substitute.For<ISqlRepository<WorkflowHistory>>();
        var employeeRepo = Substitute.For<ISqlRepository<Employee>>();
        var approvalResolver = Substitute.For<IApprovalResolver>();
        var engine = new WorkflowEngine(instanceRepo, historyRepo, employeeRepo,
            workflowBuilder, approvalResolver);

        var result = await engine.StartAsync(
            entityType, Guid.NewGuid(),
            new EmployeeId(Guid.NewGuid()), new UserId(Guid.NewGuid()),
            new UserId(Guid.NewGuid()), CancellationToken.None);

        result.IsT1.Should().BeTrue();
        result.AsT1.Code.Should().Be(HrBusinessErrorCodes.WorkflowDefinitionRequiresDefinition);
    }

    [Theory]
    [InlineData("LeaveRequest")]
    [InlineData("OvertimeRequest")]
    [InlineData("PayrollRun")]
    [InlineData("RecruitmentRequest")]
    [InlineData("EmployeeTransfer")]
    [InlineData("EmployeeSeparation")]
    [InlineData("ProbationRecord")]
    public async Task StartAsync_RequiredTypeWithDefinition_SucceedsAndBindsDefinition(string entityType)
    {
        var defId = new WorkflowDefinitionId(Guid.NewGuid());
        var stepId = new WorkflowInstanceStepId(Guid.NewGuid());
        var steps = new List<WorkflowInstanceStep>
        {
            WorkflowInstanceStep.Create(stepId, default, 1,
                ApproverType.SpecificUser, null, "user-1")
        };

        var workflowBuilder = Substitute.For<IWorkflowBuilder>();
        var buildResult = OneOf<WorkflowBuildResult, WorkflowBuildError>
            .FromT0(new WorkflowBuildResult(steps, defId, "Approval for " + entityType, 1));
        workflowBuilder.BuildAsync(Arg.Any<string>(), Arg.Any<EmployeeId>(),
                Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(buildResult);

        WorkflowInstance? createdInstance = null;
        var instanceRepo = Substitute.For<ISqlRepository<WorkflowInstance>>();
        instanceRepo.CreateOneAsync(Arg.Any<WorkflowInstance>(), Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                createdInstance = callInfo.Arg<WorkflowInstance>();
                return Task.FromResult<OneOf<WorkflowInstance, Exception>>(createdInstance!);
            });

        var historyRepo = Substitute.For<ISqlRepository<WorkflowHistory>>();
        var employeeRepo = Substitute.For<ISqlRepository<Employee>>();
        employeeRepo.GetQueryable().Returns(
            AsyncQueryableHelper.CreateMockQueryable<Employee>([]));

        var resolvedEmployeeId = new EmployeeId(Guid.NewGuid());
        var resolvedApprovers = new List<ResolvedApprover>
        {
            new(new UserId(Guid.NewGuid()), resolvedEmployeeId, "Test User", "test@test.com", "SpecificUser")
        };
        var approvalResolver = Substitute.For<IApprovalResolver>();
        approvalResolver.ResolveApproversAsync(Arg.Any<string>(), Arg.Any<string?>(),
                Arg.Any<ApprovalRoutingContext>(), Arg.Any<CancellationToken>())
            .Returns(OneOf<IReadOnlyList<ResolvedApprover>, ErrorDetailResponse>.FromT0(
                resolvedApprovers));

        var engine = new WorkflowEngine(instanceRepo, historyRepo, employeeRepo,
            workflowBuilder, approvalResolver);

        var result = await engine.StartAsync(
            entityType, Guid.NewGuid(),
            new EmployeeId(Guid.NewGuid()), new UserId(Guid.NewGuid()),
            new UserId(Guid.NewGuid()), CancellationToken.None);

        result.IsT0.Should().BeTrue();
        createdInstance.Should().NotBeNull();
        createdInstance!.WorkflowDefinitionId.Should().Be(defId);
        createdInstance.WorkflowDefinitionId.Should().NotBeNull();
        createdInstance.WorkflowDefinitionName.Should().NotBeNull();
        createdInstance.WorkflowDefinitionVersion.Should().NotBeNull();
        createdInstance.WorkflowDefinitionVersion.Should().Be(1);
    }
}
