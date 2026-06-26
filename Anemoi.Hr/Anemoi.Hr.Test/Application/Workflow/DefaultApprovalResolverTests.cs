using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Contract.Identity.ModelIds;
using Anemoi.Hr.Application.Abstractions;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Services;
using Anemoi.Hr.Domain.Departments;
using Anemoi.Hr.Domain.Employees;
using Anemoi.Hr.Domain.Workflow;
using Anemoi.Hr.ModelIds.ModelIds;
using Anemoi.Hr.Test.Helpers;
using FluentAssertions;
using NSubstitute;
using OneOf;
using Xunit;

namespace Anemoi.Hr.Test.Application.Workflow;

public sealed class DefaultApprovalResolverTests
{
    private static Employee CreateEmployee(EmployeeId id, EmployeeId? directManagerId = null,
        DepartmentId? primaryDepartmentId = null, Guid? identityUserId = null, string fullName = "Test Employee")
    {
        return new Employee
        {
            Id = id,
            DirectManagerEmployeeId = directManagerId,
            PrimaryDepartmentId = primaryDepartmentId ?? new DepartmentId(Guid.NewGuid()),
            PrimaryPositionId = new PositionId(Guid.NewGuid()),
            IdentityUserId = identityUserId,
            FullName = fullName,
            EmployeeCode = "EMP001",
            WorkEmail = "test@test.com",
            JoinDate = DateOnly.FromDateTime(DateTime.Today),
            EmploymentStatusCode = "Active",
            EmploymentTypeCode = "FullTime"
        };
    }

    [Fact]
    public async Task ResolveApproversAsync_DirectManager_ResolvesCorrectly()
    {
        var requesterId = new EmployeeId(Guid.NewGuid());
        var managerId = new EmployeeId(Guid.NewGuid());
        var managerUserId = Guid.NewGuid();

        var requester = CreateEmployee(requesterId, directManagerId: managerId);
        var manager = CreateEmployee(managerId, identityUserId: managerUserId, fullName: "Manager One");

        var employeeRepo = Substitute.For<ISqlRepository<Employee>>();
        employeeRepo.GetQueryable().Returns(
            AsyncQueryableHelper.CreateMockQueryable(new[] { requester, manager }));

        var departmentRepo = Substitute.For<ISqlRepository<Department>>();
        var roleResolver = Substitute.For<IWorkflowRoleResolver>();
        var resolver = new DefaultApprovalResolver(employeeRepo, departmentRepo, roleResolver);

        var context = new ApprovalRoutingContext(requesterId, null, null, "LeaveRequest", "1");
        var result = await resolver.ResolveApproversAsync(
            ApproverType.DirectManager, null, context, CancellationToken.None);

        result.IsT0.Should().BeTrue();
        result.AsT0.Should().HaveCount(1);
        result.AsT0[0].EmployeeId.Should().Be(managerId);
        result.AsT0[0].FullName.Should().Be("Manager One");
        result.AsT0[0].ResolutionSource.Should().Be("DirectManager");
    }

    [Fact]
    public async Task ResolveApproversAsync_DirectManager_WhenNoManager_ReturnsError()
    {
        var requesterId = new EmployeeId(Guid.NewGuid());
        var requester = CreateEmployee(requesterId, directManagerId: null);

        var employeeRepo = Substitute.For<ISqlRepository<Employee>>();
        employeeRepo.GetQueryable().Returns(
            AsyncQueryableHelper.CreateMockQueryable(new[] { requester }));

        var departmentRepo = Substitute.For<ISqlRepository<Department>>();
        var roleResolver = Substitute.For<IWorkflowRoleResolver>();
        var resolver = new DefaultApprovalResolver(employeeRepo, departmentRepo, roleResolver);

        var context = new ApprovalRoutingContext(requesterId, null, null, "LeaveRequest", "1");
        var result = await resolver.ResolveApproversAsync(
            ApproverType.DirectManager, null, context, CancellationToken.None);

        result.IsT1.Should().BeTrue();
        result.AsT1.Code.Should().Be(HrBusinessErrorCodes.WorkflowApproverNotFound);
    }

    [Fact]
    public async Task ResolveApproversAsync_DepartmentManager_ResolvesCorrectly()
    {
        var requesterId = new EmployeeId(Guid.NewGuid());
        var deptId = new DepartmentId(Guid.NewGuid());
        var deptManagerId = new EmployeeId(Guid.NewGuid());
        var managerUserId = Guid.NewGuid();

        var requester = CreateEmployee(requesterId, primaryDepartmentId: deptId);
        var department = Department.Create(deptId, "DEPT01", "Test Dept", "Division", null, deptManagerId);
        var manager = CreateEmployee(deptManagerId, identityUserId: managerUserId, fullName: "Dept Manager");

        var employeeRepo = Substitute.For<ISqlRepository<Employee>>();
        employeeRepo.GetQueryable().Returns(
            AsyncQueryableHelper.CreateMockQueryable(new[] { requester, manager }));

        var departmentRepo = Substitute.For<ISqlRepository<Department>>();
        departmentRepo.GetQueryable().Returns(
            AsyncQueryableHelper.CreateMockQueryable(new[] { department }));

        var roleResolver = Substitute.For<IWorkflowRoleResolver>();
        var resolver = new DefaultApprovalResolver(employeeRepo, departmentRepo, roleResolver);

        var context = new ApprovalRoutingContext(requesterId, null, null, "LeaveRequest", "1");
        var result = await resolver.ResolveApproversAsync(
            ApproverType.DepartmentManager, null, context, CancellationToken.None);

        result.IsT0.Should().BeTrue();
        result.AsT0.Should().HaveCount(1);
        result.AsT0[0].EmployeeId.Should().Be(deptManagerId);
        result.AsT0[0].FullName.Should().Be("Dept Manager");
        result.AsT0[0].ResolutionSource.Should().Be("DepartmentManager");
    }

    [Fact]
    public async Task ResolveApproversAsync_DepartmentManager_WhenNoDepartment_ReturnsError()
    {
        var requesterId = new EmployeeId(Guid.NewGuid());
        var requester = CreateEmployee(requesterId);

        var employeeRepo = Substitute.For<ISqlRepository<Employee>>();
        employeeRepo.GetQueryable().Returns(
            AsyncQueryableHelper.CreateMockQueryable(new[] { requester }));

        var departmentRepo = Substitute.For<ISqlRepository<Department>>();
        departmentRepo.GetQueryable().Returns(
            AsyncQueryableHelper.CreateMockQueryable<Department>([]));

        var roleResolver = Substitute.For<IWorkflowRoleResolver>();
        var resolver = new DefaultApprovalResolver(employeeRepo, departmentRepo, roleResolver);

        var context = new ApprovalRoutingContext(requesterId, null, null, "LeaveRequest", "1");
        var result = await resolver.ResolveApproversAsync(
            ApproverType.DepartmentManager, null, context, CancellationToken.None);

        result.IsT1.Should().BeTrue();
        result.AsT1.Code.Should().Be(HrBusinessErrorCodes.WorkflowApproverNotFound);
    }

    [Fact]
    public async Task ResolveApproversAsync_Role_ResolvesViaRoleResolver()
    {
        var requesterId = new EmployeeId(Guid.NewGuid());
        var roleEmployeeId = new EmployeeId(Guid.NewGuid());
        var roleUserId = Guid.NewGuid();

        var resolvedApprovers = new List<ResolvedApprover>
        {
            new(new UserId(roleUserId), roleEmployeeId, "Role Approver", "role@test.com", "Role")
        };

        var employeeRepo = Substitute.For<ISqlRepository<Employee>>();
        var departmentRepo = Substitute.For<ISqlRepository<Department>>();
        var roleResolver = Substitute.For<IWorkflowRoleResolver>();
        roleResolver.ResolveAsync("CustomRole", Arg.Any<CancellationToken>())
            .Returns(OneOf<IReadOnlyList<ResolvedApprover>, ErrorDetailResponse>.FromT0(resolvedApprovers));

        var resolver = new DefaultApprovalResolver(employeeRepo, departmentRepo, roleResolver);

        var context = new ApprovalRoutingContext(requesterId, null, null, "LeaveRequest", "1");
        var result = await resolver.ResolveApproversAsync(
            ApproverType.Role, "CustomRole", context, CancellationToken.None);

        result.IsT0.Should().BeTrue();
        result.AsT0.Should().HaveCount(1);
        result.AsT0[0].EmployeeId.Should().Be(roleEmployeeId);
        result.AsT0[0].FullName.Should().Be("Role Approver");
    }

    [Fact]
    public async Task ResolveApproversAsync_HrManager_ResolvesViaRoleResolver()
    {
        var requesterId = new EmployeeId(Guid.NewGuid());
        var hrEmployeeId = new EmployeeId(Guid.NewGuid());
        var hrUserId = Guid.NewGuid();

        var resolvedApprovers = new List<ResolvedApprover>
        {
            new(new UserId(hrUserId), hrEmployeeId, "HR Manager", "hr@test.com", "HrManager")
        };

        var employeeRepo = Substitute.For<ISqlRepository<Employee>>();
        var departmentRepo = Substitute.For<ISqlRepository<Department>>();
        var roleResolver = Substitute.For<IWorkflowRoleResolver>();
        roleResolver.ResolveAsync(WorkflowRole.HrManager, Arg.Any<CancellationToken>())
            .Returns(OneOf<IReadOnlyList<ResolvedApprover>, ErrorDetailResponse>.FromT0(resolvedApprovers));

        var resolver = new DefaultApprovalResolver(employeeRepo, departmentRepo, roleResolver);

        var context = new ApprovalRoutingContext(requesterId, null, null, "LeaveRequest", "1");
        var result = await resolver.ResolveApproversAsync(
            ApproverType.HrManager, null, context, CancellationToken.None);

        result.IsT0.Should().BeTrue();
        result.AsT0.Should().HaveCount(1);
        result.AsT0[0].EmployeeId.Should().Be(hrEmployeeId);
        result.AsT0[0].FullName.Should().Be("HR Manager");
    }

    [Fact]
    public async Task ResolveApproversAsync_HrManager_WhenNoRoleAssignment_ReturnsError()
    {
        var requesterId = new EmployeeId(Guid.NewGuid());

        var employeeRepo = Substitute.For<ISqlRepository<Employee>>();
        var departmentRepo = Substitute.For<ISqlRepository<Department>>();
        var roleResolver = Substitute.For<IWorkflowRoleResolver>();
        roleResolver.ResolveAsync(WorkflowRole.HrManager, Arg.Any<CancellationToken>())
            .Returns(HrErrorResponses.Create(HrBusinessErrorCodes.WorkflowApproverNotFound));

        var resolver = new DefaultApprovalResolver(employeeRepo, departmentRepo, roleResolver);

        var context = new ApprovalRoutingContext(requesterId, null, null, "LeaveRequest", "1");
        var result = await resolver.ResolveApproversAsync(
            ApproverType.HrManager, null, context, CancellationToken.None);

        result.IsT1.Should().BeTrue();
        result.AsT1.Code.Should().Be(HrBusinessErrorCodes.WorkflowApproverNotFound);
    }
}
