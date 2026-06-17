using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.Contract.Identity.ModelIds;
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

        var seen = new HashSet<UserId>();
        var steps = new List<ResolvedApproverStep>();
        var stepOrder = 0;

        // Step 1: Direct Manager
        if (employee.DirectManagerEmployeeId is not null)
        {
            var manager = await employeeRepository.GetQueryable()
                .FirstOrDefaultAsync(e => e.Id == employee.DirectManagerEmployeeId, ct);
            if (manager?.IdentityUserId is not null && seen.Add(new UserId(manager.IdentityUserId.Value)))
                steps.Add(new ResolvedApproverStep(++stepOrder,
                    ApproverType.SpecificUser, null, new UserId(manager.IdentityUserId.Value)));
        }

        // Step 2+: Walk department hierarchy chain, collecting IDs
        var managerEmployeeIds = new List<EmployeeId>();
        var currentDeptId = employee.PrimaryDepartmentId;
        while (currentDeptId is not null)
        {
            var dept = await departmentRepository.GetQueryable()
                .FirstOrDefaultAsync(d => d.Id == currentDeptId, ct);
            if (dept is null) break;
            if (dept.ManagerEmployeeId is not null)
                managerEmployeeIds.Add(dept.ManagerEmployeeId);
            currentDeptId = dept.ParentDepartmentId;
        }

        // Batch-load all manager employees in one query
        var managers = managerEmployeeIds.Count > 0
            ? await employeeRepository.GetQueryable()
                .Where(e => managerEmployeeIds.Contains(e.Id))
                .ToDictionaryAsync(e => e.Id, ct)
            : [];

        // Build steps from department chain, deduplicating
        foreach (var mgrId in managerEmployeeIds)
        {
            if (!managers.TryGetValue(mgrId, out var mgr) || mgr.IdentityUserId is null)
                continue;
            var userId = new UserId(mgr.IdentityUserId.Value);
            if (seen.Add(userId))
                steps.Add(new ResolvedApproverStep(++stepOrder,
                    ApproverType.SpecificUser, null, userId));
        }

        return steps;
    }
}
