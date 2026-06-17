using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.Contract.Identity.ModelIds;
using Anemoi.Hr.Application.Services;
using Anemoi.Hr.Domain.Departments;
using Anemoi.Hr.Domain.Employees;
using Anemoi.Hr.ModelIds.ModelIds;
using Anemoi.Hr.Test.Helpers;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Anemoi.Hr.Test.Application.Workflow;

public sealed class WorkflowHierarchyResolverTests
{
    [Fact]
    public async Task ResolveHierarchyAsync_SameManagerAsDirectAndDepartment_Deduplicates()
    {
        var managerUserId = Guid.NewGuid();
        var managerId = new EmployeeId(Guid.NewGuid());
        var employeeId = new EmployeeId(Guid.NewGuid());
        var deptId = new DepartmentId(Guid.NewGuid());

        var manager = new Employee
        {
            Id = managerId,
            IdentityUserId = managerUserId,
            FullName = "Manager User"
        };

        var employee = new Employee
        {
            Id = employeeId,
            DirectManagerEmployeeId = managerId,
            PrimaryDepartmentId = deptId
        };

        var department = new Department
        {
            Id = deptId,
            Name = "Test Dept",
            ManagerEmployeeId = managerId
        };

        var employeeRepo = Substitute.For<ISqlRepository<Employee>>();
        employeeRepo.GetQueryable().Returns(
            AsyncQueryableHelper.CreateMockQueryable(new[] { employee, manager }));

        var departmentRepo = Substitute.For<ISqlRepository<Department>>();
        departmentRepo.GetQueryable().Returns(
            AsyncQueryableHelper.CreateMockQueryable(new[] { department }));

        var resolver = new WorkflowHierarchyResolver(employeeRepo, departmentRepo);

        var result = await resolver.ResolveHierarchyAsync(employeeId, CancellationToken.None);

        result.Should().HaveCount(1);
        result.First().ApproverUserId.Should().Be(new UserId(managerUserId));
    }

    [Fact]
    public async Task ResolveHierarchyAsync_EmployeeWithNoManager_ReturnsEmpty()
    {
        var employeeId = new EmployeeId(Guid.NewGuid());
        var deptId = new DepartmentId(Guid.NewGuid());

        var employee = new Employee
        {
            Id = employeeId,
            DirectManagerEmployeeId = null,
            PrimaryDepartmentId = deptId
        };

        var department = new Department
        {
            Id = deptId,
            Name = "Test Dept",
            ManagerEmployeeId = null
        };

        var employeeRepo = Substitute.For<ISqlRepository<Employee>>();
        employeeRepo.GetQueryable().Returns(
            AsyncQueryableHelper.CreateMockQueryable(new[] { employee, employee }));

        var departmentRepo = Substitute.For<ISqlRepository<Department>>();
        departmentRepo.GetQueryable().Returns(
            AsyncQueryableHelper.CreateMockQueryable(new[] { department }));

        var resolver = new WorkflowHierarchyResolver(employeeRepo, departmentRepo);

        var result = await resolver.ResolveHierarchyAsync(employeeId, CancellationToken.None);

        result.Should().BeEmpty();
    }
}
