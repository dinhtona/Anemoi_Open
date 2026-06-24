using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.Hr.Application.Abstractions;
using Anemoi.Hr.Domain.Employees;
using Anemoi.Hr.Domain.Workflow;
using Anemoi.Hr.ModelIds.ModelIds;
using Microsoft.EntityFrameworkCore;

namespace Anemoi.Hr.Infrastructure.Services;

public sealed class WorkflowQueryService(
    ISqlRepository<WorkflowInstance> workflowRepo,
    ISqlRepository<Employee> employeeRepo)
    : IWorkflowQueryService
{
    public async Task<Dictionary<Guid, WorkflowSummaryResponse>> GetWorkflowSummariesAsync(
        string entityType, IReadOnlyCollection<Guid> entityIds, CancellationToken ct)
    {
        if (entityIds is null || entityIds.Count == 0)
            return [];

        var entityIdStrings = entityIds.Select(id => id.ToString()).ToHashSet();

        var pendingStatuses = new HashSet<string>
        {
            WorkflowStatusCode.Pending,
            WorkflowStatusCode.Returned
        };

        var instances = await workflowRepo.GetQueryable()
            .Include(x => x.Steps)
            .Where(x => x.EntityType == entityType
                     && entityIdStrings.Contains(x.EntityId)
                     && pendingStatuses.Contains(x.Status))
            .ToListAsync(ct);

        if (instances.Count == 0)
            return [];

        var approverEmployeeIds = instances
            .Select(instance =>
            {
                var step = instance.Steps.FirstOrDefault(s => s.Sequence == instance.CurrentStep);
                return step?.ApproverEmployeeId;
            })
            .Where(id => id is not null)
            .Select(id => (EmployeeId)id!)
            .Distinct()
            .ToList();

        var employeeNames = new Dictionary<EmployeeId, string>();
        if (approverEmployeeIds.Count > 0)
        {
            var employeeRows = await employeeRepo.GetQueryable()
                .Where(e => approverEmployeeIds.Contains(e.Id))
                .ToListAsync(ct);
            foreach (var emp in employeeRows)
                employeeNames[emp.Id] = emp.FullName;
        }

        var result = new Dictionary<Guid, WorkflowSummaryResponse>();
        foreach (var instance in instances)
        {
            if (!Guid.TryParse(instance.EntityId, out var entityGuid))
                continue;

            var step = instance.Steps.FirstOrDefault(s => s.Sequence == instance.CurrentStep);

            string? approverName = null;
            if (step?.ApproverEmployeeId is not null
                && employeeNames.TryGetValue(step.ApproverEmployeeId, out var name))
            {
                approverName = name;
            }

            result[entityGuid] = new WorkflowSummaryResponse(
                approverName,
                step?.Status is not null ? GetStepDisplayName(step) : null,
                instance.Status);
        }

        return result;
    }

    private static string? GetStepDisplayName(WorkflowInstanceStep step)
    {
        return step.ApproverTypeSnapshot switch
        {
            ApproverType.DirectManager => "Direct Manager Approval",
            ApproverType.DepartmentManager => "Department Manager Approval",
            ApproverType.HrManager => "HR Manager Approval",
            ApproverType.SpecificUser => "Specific Approver",
            ApproverType.Role => $"Role: {step.ApproverValueSnapshot}",
            ApproverType.Permission => $"Permission: {step.ApproverValueSnapshot}",
            _ => $"Step {step.Sequence}"
        };
    }
}
