using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Helpers;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Abstractions;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Models;
using Anemoi.Hr.Domain.Departments;
using Anemoi.Hr.Domain.Employees;
using Anemoi.Hr.Domain.Organization;
using Anemoi.Hr.Domain.Workflow;
using Anemoi.Hr.ModelIds.ModelIds;
using Microsoft.EntityFrameworkCore;
using OneOf;

namespace Anemoi.Hr.Application.Services;

public interface IOrganizationService
{
    Task<List<OrganizationNode>> GetOrganizationTreeAsync(CancellationToken ct);
    Task<List<ReportingRelationship>> GetReportingChainAsync(EmployeeId employeeId, CancellationToken ct);
    Task<OneOf<bool, ErrorDetailResponse>> UpdateManagerAsync(EmployeeId employeeId, EmployeeId? managerId, CancellationToken ct);
    Task<List<WorkflowApproverCandidate>> GetApproversPreviewAsync(EmployeeId employeeId, string entityType, CancellationToken ct);
    Task<List<WorkflowApproverCandidate>> GetWorkflowRoutePreviewAsync(EmployeeId employeeId, string entityType, CancellationToken ct);
}

public sealed class OrganizationService(
    ISqlRepository<Employee> employeeRepository,
    ISqlRepository<Department> departmentRepository,
    ISqlRepository<WorkflowDefinition> definitionRepository,
    IApprovalResolver approvalResolver,
    IWorkflowHierarchyResolver hierarchyResolver,
    IUnitOfWork unitOfWork)
    : IOrganizationService
{
    public async Task<List<OrganizationNode>> GetOrganizationTreeAsync(CancellationToken ct)
    {
        var employees = await employeeRepository.GetQueryable()
            .Include(e => e.PrimaryDepartment)
            .Include(e => e.PrimaryPosition)
            .ToListAsync(ct);

        var departments = await departmentRepository.GetQueryable()
            .ToDictionaryAsync(d => d.Id, ct);

        var employeeMap = employees.ToDictionary(e => e.Id);

        var roots = employees
            .Where(e => e.DirectManagerEmployeeId is null)
            .OrderBy(e => e.FullName)
            .ToList();

        var tree = roots.Select(r => BuildNode(r, employees, departments)).ToList();
        return tree;
    }

    private OrganizationNode BuildNode(Employee employee, List<Employee> allEmployees,
        Dictionary<DepartmentId, Department> departments)
    {
        var dept = employee.PrimaryDepartmentId is not null
            ? departments.GetValueOrDefault(employee.PrimaryDepartmentId)
            : null;

        var manager = employee.DirectManagerEmployeeId is not null
            ? allEmployees.FirstOrDefault(e => e.Id == employee.DirectManagerEmployeeId)
            : null;

        var directReports = allEmployees
            .Where(e => e.DirectManagerEmployeeId == employee.Id)
            .OrderBy(e => e.FullName)
            .ToList();

        return new OrganizationNode(
            employee.Id,
            employee.FullName,
            employee.EmployeeCode,
            employee.DirectManagerEmployeeId,
            manager?.FullName,
            employee.PrimaryDepartmentId,
            dept?.Name ?? string.Empty,
            employee.PrimaryPosition?.Name ?? string.Empty,
            employee.GradeCode,
            directReports.Select(r => BuildNode(r, allEmployees, departments)).ToList());
    }

    public async Task<List<ReportingRelationship>> GetReportingChainAsync(
        EmployeeId employeeId, CancellationToken ct)
    {
        var employees = await employeeRepository.GetQueryable()
            .ToDictionaryAsync(e => e.Id, ct);

        if (!employees.TryGetValue(employeeId, out var current))
            return [];

        var chain = new List<ReportingRelationship>();

        var level = 0;
        var levels = new[] { HierarchyLevel.Employee, HierarchyLevel.DirectManager,
            HierarchyLevel.DepartmentManager, HierarchyLevel.HrManager };
        chain.Add(new ReportingRelationship(current.Id, current.FullName,
            current.DirectManagerEmployeeId,
            current.DirectManagerEmployeeId is not null && employees.TryGetValue(current.DirectManagerEmployeeId, out var dm) ? dm.FullName : null,
            level < levels.Length ? levels[level] : $"Level{level}"));

        var visited = new HashSet<EmployeeId> { current.Id };
        var managerId = current.DirectManagerEmployeeId;
        level++;

        while (managerId is not null && !visited.Contains(managerId))
        {
            visited.Add(managerId);
            if (!employees.TryGetValue(managerId, out var manager))
                break;

            chain.Add(new ReportingRelationship(manager.Id, manager.FullName,
                manager.DirectManagerEmployeeId,
                manager.DirectManagerEmployeeId is not null && employees.TryGetValue(manager.DirectManagerEmployeeId, out var mgr) ? mgr.FullName : null,
                level < levels.Length ? levels[level] : $"Level{level}"));

            managerId = manager.DirectManagerEmployeeId;
            level++;
        }

        return chain;
    }

    public async Task<OneOf<bool, ErrorDetailResponse>> UpdateManagerAsync(
        EmployeeId employeeId, EmployeeId? managerId, CancellationToken ct)
    {
        var employee = await employeeRepository.GetQueryable()
            .FirstOrDefaultAsync(e => e.Id == employeeId, ct);

        if (employee is null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.EmployeeNotFound);

        if (managerId is not null)
        {
            var manager = await employeeRepository.GetQueryable()
                .FirstOrDefaultAsync(e => e.Id == managerId, ct);
            if (manager is null)
                return HrErrorResponses.Create(HrBusinessErrorCodes.EmployeeNotFound);

            if (managerId == employeeId)
                return HrErrorResponses.Create(HrBusinessErrorCodes.WorkflowApproverNotFound);

            // Detect circular hierarchy
            var visited = new HashSet<EmployeeId> { employeeId };
            var currentManagerId = manager.DirectManagerEmployeeId;
            while (currentManagerId is not null)
            {
                if (!visited.Add(currentManagerId))
                    return HrErrorResponses.Create(HrBusinessErrorCodes.WorkflowApproverNotFound);
                var mgr = await employeeRepository.GetQueryable()
                    .FirstOrDefaultAsync(e => e.Id == currentManagerId, ct);
                if (mgr is null) break;
                currentManagerId = mgr.DirectManagerEmployeeId;
            }
        }

        employee.DirectManagerEmployeeId = managerId;
        employee.UpdatedAt = DateTime.UtcNow;
        await unitOfWork.SaveChangesAsync(ct);

        return true;
    }

    public async Task<List<WorkflowApproverCandidate>> GetApproversPreviewAsync(
        EmployeeId employeeId, string entityType, CancellationToken ct)
    {
        var employee = await employeeRepository.GetQueryable()
            .Include(e => e.PrimaryDepartment)
            .FirstOrDefaultAsync(e => e.Id == employeeId, ct);
        if (employee is null) return [];

        var context = new ApprovalRoutingContext(
            employeeId, employee.PrimaryDepartmentId, null, entityType, null);

        var candidates = new List<WorkflowApproverCandidate>();

        var hierarchy = await hierarchyResolver.ResolveHierarchyAsync(employeeId, ct);
        var maxSteps = WorkflowConstants.DefaultPolicy.GetStepCount(entityType);
        var selected = hierarchy.Take(maxSteps > 0 ? maxSteps : 10).ToList();

        foreach (var step in selected)
        {
            var result = await approvalResolver.ResolveApproversAsync(
                step.ApproverType, step.ApproverValue, context, ct);

            if (result.TryPickT0(out var approvers, out _) && approvers.Count > 0)
            {
                candidates.Add(new WorkflowApproverCandidate(
                    step.StepOrder,
                    step.ApproverType,
                    step.ApproverValue,
                    approvers[0].EmployeeId,
                    approvers[0].FullName,
                    approvers[0].Email));
            }
            else
            {
                candidates.Add(new WorkflowApproverCandidate(
                    step.StepOrder,
                    step.ApproverType,
                    step.ApproverValue,
                    null, "Unable to resolve", null));
            }
        }

        return candidates;
    }

    public async Task<List<WorkflowApproverCandidate>> GetWorkflowRoutePreviewAsync(
        EmployeeId employeeId, string entityType, CancellationToken ct)
    {
        var employee = await employeeRepository.GetQueryable()
            .Include(e => e.PrimaryDepartment)
            .FirstOrDefaultAsync(e => e.Id == employeeId, ct);
        if (employee is null) return [];

        var definition = await definitionRepository.GetQueryable()
            .Include(d => d.Steps)
            .Where(d => d.TargetEntityType == entityType && d.IsActive)
            .FirstOrDefaultAsync(ct);

        var context = new ApprovalRoutingContext(
            employeeId, employee.PrimaryDepartmentId, null, entityType, null);

        var candidates = new List<WorkflowApproverCandidate>();

        if (definition is not null)
        {
            var sortedSteps = definition.Steps.OrderBy(s => s.Sequence).ToList();
            foreach (var step in sortedSteps)
            {
                var result = await approvalResolver.ResolveApproversAsync(
                    step.ApproverType, step.ApproverValue, context, ct);

                if (result.TryPickT0(out var approvers, out _) && approvers.Count > 0)
                {
                    candidates.Add(new WorkflowApproverCandidate(
                        step.Sequence,
                        step.ApproverType,
                        step.ApproverValue,
                        approvers[0].EmployeeId,
                        approvers[0].FullName,
                        approvers[0].Email));
                }
                else
                {
                    candidates.Add(new WorkflowApproverCandidate(
                        step.Sequence,
                        step.ApproverType,
                        step.ApproverValue,
                        null, "Unable to resolve", null));
                }
            }
        }
        else if (!WorkflowConstants.DefaultPolicy.RequiresDefinition(entityType))
        {
            return await GetApproversPreviewAsync(employeeId, entityType, ct);
        }

        return candidates;
    }
}
