using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.Hr.Application.Abstractions;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Domain.Employees;
using Anemoi.Hr.Domain.Workflow;
using Anemoi.Hr.Infrastructure.Services;
using Anemoi.Hr.ModelIds.ModelIds;
using Anemoi.Hr.Test.Helpers;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Anemoi.Hr.Test.Application.Workflow;

public sealed class WorkflowQueryServiceTests
{
    private static Employee CreateEmployee(EmployeeId id, string fullName)
    {
        return new Employee
        {
            Id = id,
            FullName = fullName,
            EmployeeCode = "EMP001",
            WorkEmail = "test@test.com",
            PrimaryDepartmentId = new DepartmentId(Guid.NewGuid()),
            PrimaryPositionId = new PositionId(Guid.NewGuid()),
            JoinDate = DateOnly.FromDateTime(DateTime.Today),
            EmploymentStatusCode = "Active",
            EmploymentTypeCode = "FullTime"
        };
    }

    [Fact]
    public async Task GetWorkflowSummariesAsync_ReturnsInfoForAllStatuses()
    {
        var entityId1 = Guid.NewGuid();
        var entityId2 = Guid.NewGuid();
        var approverId = new EmployeeId(Guid.NewGuid());

        var approvedStep = WorkflowInstanceStep.Create(
            new WorkflowInstanceStepId(Guid.NewGuid()), default, 1,
            ApproverType.DirectManager, null, null);
        approvedStep.SetApprover(approverId, "user-1");

        var approvedInstance = WorkflowInstance.Start(
            new WorkflowInstanceId(Guid.NewGuid()), null,
            "LeaveRequest", entityId1.ToString(), "startedBy",
            new EmployeeId(Guid.NewGuid()), new Anemoi.Contract.Identity.ModelIds.UserId(Guid.NewGuid()),
            new List<WorkflowInstanceStep> { approvedStep });
        approvedInstance.Approve(new WorkflowHistoryId(Guid.NewGuid()), "user-1", null);

        var pendingStep = WorkflowInstanceStep.Create(
            new WorkflowInstanceStepId(Guid.NewGuid()), default, 1,
            ApproverType.DepartmentManager, null, null);
        pendingStep.SetApprover(approverId, "user-1");

        var pendingInstance = WorkflowInstance.Start(
            new WorkflowInstanceId(Guid.NewGuid()), null,
            "LeaveRequest", entityId2.ToString(), "startedBy",
            new EmployeeId(Guid.NewGuid()), new Anemoi.Contract.Identity.ModelIds.UserId(Guid.NewGuid()),
            new List<WorkflowInstanceStep> { pendingStep });

        var instances = new List<WorkflowInstance> { approvedInstance, pendingInstance };
        var workflowRepo = Substitute.For<ISqlRepository<WorkflowInstance>>();
        workflowRepo.GetQueryable().Returns(
            AsyncQueryableHelper.CreateMockQueryable(instances));

        var approverEmployee = CreateEmployee(approverId, "Approver Name");

        var employeeRepo = Substitute.For<ISqlRepository<Employee>>();
        employeeRepo.GetQueryable().Returns(
            AsyncQueryableHelper.CreateMockQueryable(new[] { approverEmployee }));

        var service = new WorkflowQueryService(workflowRepo, employeeRepo);

        var result = await service.GetWorkflowSummariesAsync(
            "LeaveRequest",
            new[] { entityId1, entityId2 },
            CancellationToken.None);

        result.Should().HaveCount(2);
        result.Should().ContainKey(entityId1);
        result.Should().ContainKey(entityId2);
        result[entityId1].WorkflowStatus.Should().Be(WorkflowStatusCode.Approved);
        result[entityId2].WorkflowStatus.Should().Be(WorkflowStatusCode.Pending);
        result[entityId1].CurrentApproverName.Should().Be("Approver Name");
        result[entityId2].CurrentApproverName.Should().Be("Approver Name");
    }

    [Fact]
    public async Task GetWorkflowSummariesAsync_EmptyIds_ReturnsEmpty()
    {
        var workflowRepo = Substitute.For<ISqlRepository<WorkflowInstance>>();
        var employeeRepo = Substitute.For<ISqlRepository<Employee>>();
        var service = new WorkflowQueryService(workflowRepo, employeeRepo);

        var result = await service.GetWorkflowSummariesAsync(
            "LeaveRequest", [], CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetWorkflowSummariesAsync_NoInstances_ReturnsEmpty()
    {
        var workflowRepo = Substitute.For<ISqlRepository<WorkflowInstance>>();
        workflowRepo.GetQueryable().Returns(
            AsyncQueryableHelper.CreateMockQueryable<WorkflowInstance>([]));

        var employeeRepo = Substitute.For<ISqlRepository<Employee>>();
        var service = new WorkflowQueryService(workflowRepo, employeeRepo);

        var result = await service.GetWorkflowSummariesAsync(
            "LeaveRequest", [Guid.NewGuid()], CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetWorkflowSummariesAsync_ReturnsStepNameForPendingStep()
    {
        var entityId = Guid.NewGuid();
        var approverId = new EmployeeId(Guid.NewGuid());

        var step = WorkflowInstanceStep.Create(
            new WorkflowInstanceStepId(Guid.NewGuid()), default, 1,
            ApproverType.DirectManager, null, null);
        step.SetApprover(approverId, "user-1");

        var instance = WorkflowInstance.Start(
            new WorkflowInstanceId(Guid.NewGuid()), null,
            "LeaveRequest", entityId.ToString(), "startedBy",
            new EmployeeId(Guid.NewGuid()), new Anemoi.Contract.Identity.ModelIds.UserId(Guid.NewGuid()),
            new List<WorkflowInstanceStep> { step });

        var workflowRepo = Substitute.For<ISqlRepository<WorkflowInstance>>();
        workflowRepo.GetQueryable().Returns(
            AsyncQueryableHelper.CreateMockQueryable(new[] { instance }));

        var approverEmployee = CreateEmployee(approverId, "Direct Manager Name");

        var employeeRepo = Substitute.For<ISqlRepository<Employee>>();
        employeeRepo.GetQueryable().Returns(
            AsyncQueryableHelper.CreateMockQueryable(new[] { approverEmployee }));

        var service = new WorkflowQueryService(workflowRepo, employeeRepo);

        var result = await service.GetWorkflowSummariesAsync(
            "LeaveRequest", [entityId], CancellationToken.None);

        result.Should().HaveCount(1);
        result[entityId].CurrentStepName.Should().Be("Direct Manager Approval");
        result[entityId].CurrentApproverName.Should().Be("Direct Manager Name");
        result[entityId].WorkflowStatus.Should().Be(WorkflowStatusCode.Pending);
    }
}
