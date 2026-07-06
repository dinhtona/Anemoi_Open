using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.Hr.Application.Abstractions;
using Anemoi.Hr.Domain.Departments;
using Anemoi.Hr.Domain.Employees;
using Anemoi.Hr.Domain.Workflow;
using Anemoi.Hr.ModelIds.ModelIds;
using Microsoft.EntityFrameworkCore;

namespace Anemoi.Hr.Application.Services;

public sealed class WorkflowHierarchyResolver(
    ISqlRepository<Employee> employeeRepository,
    ISqlRepository<Department> departmentRepository)
    : IWorkflowHierarchyResolver
{
    public async Task<IReadOnlyList<ResolvedApproverStep>> ResolveHierarchyAsync(
        EmployeeId requesterEmployeeId, CancellationToken ct)
    {
        var employee = await employeeRepository.GetQueryable()
            .FirstOrDefaultAsync(e => e.Id == requesterEmployeeId, ct);

        if (employee is null)
            return [];

        var steps = new List<ResolvedApproverStep>();
        var stepOrder = 0;

        // Step 1: Direct Manager (resolved dynamically at step activation)
        if (employee.DirectManagerEmployeeId is not null)
            steps.Add(new ResolvedApproverStep(++stepOrder,
                ApproverType.DirectManager, null));

        // Step 2+: Walk department hierarchy chain (each level = DepartmentManager)
        var currentDeptId = employee.PrimaryDepartmentId;
        while (currentDeptId is not null)
        {
            var dept = await departmentRepository.GetQueryable()
                .FirstOrDefaultAsync(d => d.Id == currentDeptId, ct);
            if (dept is null) break;
            if (dept.ManagerEmployeeId is not null)
                steps.Add(new ResolvedApproverStep(++stepOrder,
                    ApproverType.DepartmentManager, null));
            currentDeptId = dept.ParentDepartmentId;
        }

        return steps;
    }
}
