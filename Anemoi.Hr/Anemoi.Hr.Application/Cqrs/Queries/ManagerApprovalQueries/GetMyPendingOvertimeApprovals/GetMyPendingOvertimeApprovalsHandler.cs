using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Abstractions;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Departments;
using Anemoi.Hr.Domain.Employees;
using Anemoi.Hr.Domain.Overtime;
using Anemoi.Hr.Domain.Workflow;
using Anemoi.Hr.ModelIds.ModelIds;
using Microsoft.EntityFrameworkCore;
using OneOf;

namespace Anemoi.Hr.Application.Cqrs.Queries.ManagerApprovalQueries.GetMyPendingOvertimeApprovals;

public sealed class GetMyPendingOvertimeApprovalsHandler(
    ISqlRepository<WorkflowInstance> instanceRepository,
    ISqlRepository<OvertimeRequest> overtimeRequestRepository,
    ISqlRepository<Employee> employeeRepository,
    IWorkflowRoleResolver roleResolver)
    : IQueryHandler<GetMyPendingOvertimeApprovalsQuery,
        OneOf<IReadOnlyCollection<ManagerOvertimePendingApprovalResponse>, ErrorDetailResponse>>
{
    public async Task<OneOf<IReadOnlyCollection<ManagerOvertimePendingApprovalResponse>, ErrorDetailResponse>> Handle(
        GetMyPendingOvertimeApprovalsQuery request, CancellationToken cancellationToken)
    {
        var pendingInstances = await instanceRepository.GetQueryable()
            .Include(x => x.Steps)
            .Where(x => x.EntityType == WorkflowConstants.TargetEntityTypes.OvertimeRequest
                     && x.Status == WorkflowStatusCode.Pending)
            .OrderByDescending(x => x.StartedAt)
            .ToListAsync(cancellationToken);

        var matchedIds = await FilterInstancesForUserAsync(pendingInstances, request.UserId, cancellationToken);
        if (matchedIds.Count == 0)
            return Array.Empty<ManagerOvertimePendingApprovalResponse>();

        var entityIds = matchedIds
            .Select(i => new OvertimeRequestId(Guid.Parse(i.EntityId)))
            .Distinct()
            .ToList();

        var overtimeRequests = await overtimeRequestRepository.GetQueryable()
            .Include(x => x.Employee)
            .Where(x => entityIds.Contains(x.Id))
            .ToListAsync(cancellationToken);

        var employeeIds = overtimeRequests
            .Select(x => x.EmployeeId)
            .Distinct()
            .ToList();

        var employees = await employeeRepository.GetQueryable()
            .Include(e => e.PrimaryDepartment)
            .Where(e => employeeIds.Contains(e.Id))
            .ToDictionaryAsync(e => e.Id, cancellationToken);

        var workflowByEntityId = matchedIds
            .GroupBy(x => x.EntityId)
            .ToDictionary(
                g => Guid.Parse(g.Key),
                g =>
                {
                    var instance = g.First();
                    var step = instance.Steps.FirstOrDefault(s => s.Sequence == instance.CurrentStep);
                    return (instance, step);
                });

        var approverEmployeeIds = workflowByEntityId.Values
            .Select(v => v.step?.ApproverEmployeeId)
            .Where(id => id is not null)
            .Select(id => (EmployeeId)id!)
            .Distinct()
            .ToList();

        var approverNames = new Dictionary<EmployeeId, string>();
        if (approverEmployeeIds.Count > 0)
        {
            var approverRows = await employeeRepository.GetQueryable()
                .Where(e => approverEmployeeIds.Contains(e.Id))
                .ToListAsync(cancellationToken);
            foreach (var emp in approverRows)
                approverNames[emp.Id] = emp.FullName;
        }

        var results = new List<ManagerOvertimePendingApprovalResponse>();
        foreach (var overtimeRequest in overtimeRequests)
        {
            if (!workflowByEntityId.TryGetValue(overtimeRequest.Id.Value, out var workflowInfo))
                continue;

            var (instance, step) = workflowInfo;
            employees.TryGetValue(overtimeRequest.EmployeeId, out var employee);

            var stepName = GetStepDisplayName(step);

            string? approverName = null;
            if (step?.ApproverEmployeeId is not null)
                approverNames.TryGetValue(step.ApproverEmployeeId, out approverName);

            results.Add(new ManagerOvertimePendingApprovalResponse
            {
                RequestId = overtimeRequest.Id.Value.ToString(),
                EmployeeId = overtimeRequest.EmployeeId.Value.ToString(),
                EmployeeName = employee?.FullName ?? overtimeRequest.Employee?.FullName,
                DepartmentName = employee?.PrimaryDepartment?.Name,
                OvertimeDate = overtimeRequest.OvertimeDate,
                Hours = overtimeRequest.CalculateDurationHours(),
                Reason = overtimeRequest.Reason,
                SubmittedAt = overtimeRequest.CreatedAt,
                Status = overtimeRequest.Status,
                WorkflowInstanceId = instance.Id.Value.ToString(),
                CurrentStepName = stepName,
                WorkflowStatus = instance.Status,
                CurrentApproverName = approverName
            });
        }

        return results.OrderByDescending(r => r.SubmittedAt).ToList();
    }

    private async Task<List<WorkflowInstance>> FilterInstancesForUserAsync(
        List<WorkflowInstance> instances, string? userId, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(userId))
            return [];

        var roleValues = instances
            .Select(i => i.Steps.FirstOrDefault(s => s.Sequence == i.CurrentStep))
            .Where(s => s?.ApproverTypeSnapshot == ApproverType.Role && s.ApproverValueSnapshot is not null)
            .Select(s => s!.ApproverValueSnapshot)
            .Distinct()
            .ToList();

        var userRoleValues = new HashSet<string>();
        foreach (var role in roleValues)
        {
            var result = await roleResolver.ResolveAsync(role, ct);
            if (result.TryPickT0(out var approvers, out _) &&
                approvers.Any(a => a.UserId.Value.ToString() == userId))
            {
                userRoleValues.Add(role);
            }
        }

        return instances.Where(instance =>
        {
            var step = instance.Steps.FirstOrDefault(s => s.Sequence == instance.CurrentStep);
            if (step is null) return false;

            return step.ApproverTypeSnapshot switch
            {
                ApproverType.SpecificUser => step.ApproverValueSnapshot == userId,
                ApproverType.DirectManager => step.ApproverUserId == userId,
                ApproverType.DepartmentManager => step.ApproverUserId == userId,
                ApproverType.HrManager => step.ApproverUserId == userId,
                ApproverType.Role => userRoleValues.Contains(step.ApproverValueSnapshot),
                ApproverType.Permission => true,
                _ => false
            };
        }).ToList();
    }

    private static string GetStepDisplayName(WorkflowInstanceStep? step)
    {
        if (step is null) return "Unknown";

        return step.ApproverTypeSnapshot switch
        {
            ApproverType.DirectManager => "Direct Manager Approval",
            ApproverType.DepartmentManager => "Department Manager Approval",
            ApproverType.HrManager => "HR Manager Approval",
            ApproverType.SpecificUser => "Specific Approver",
            ApproverType.Role => $"Role: {step.ApproverValueSnapshot}",
            ApproverType.Permission => "Permission Approval",
            _ => $"Step {step.Sequence}"
        };
    }
}
