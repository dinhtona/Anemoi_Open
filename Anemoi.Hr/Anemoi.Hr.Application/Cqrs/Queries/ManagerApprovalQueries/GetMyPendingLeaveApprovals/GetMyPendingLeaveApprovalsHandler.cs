using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Abstractions;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Departments;
using Anemoi.Hr.Domain.Employees;
using Anemoi.Hr.Domain.Leaves;
using Anemoi.Hr.Domain.Workflow;
using Anemoi.Hr.ModelIds.ModelIds;
using Microsoft.EntityFrameworkCore;
using OneOf;

namespace Anemoi.Hr.Application.Cqrs.Queries.ManagerApprovalQueries.GetMyPendingLeaveApprovals;

public sealed class GetMyPendingLeaveApprovalsHandler(
    ISqlRepository<WorkflowInstance> instanceRepository,
    ISqlRepository<LeaveRequest> leaveRequestRepository,
    ISqlRepository<Employee> employeeRepository,
    IWorkflowRoleResolver roleResolver,
    IPermissionResolver permissionResolver)
    : IQueryHandler<GetMyPendingLeaveApprovalsQuery,
        OneOf<IReadOnlyCollection<ManagerLeavePendingApprovalResponse>, ErrorDetailResponse>>
{
    public async Task<OneOf<IReadOnlyCollection<ManagerLeavePendingApprovalResponse>, ErrorDetailResponse>> Handle(
        GetMyPendingLeaveApprovalsQuery request, CancellationToken cancellationToken)
    {
        var pendingInstances = await instanceRepository.GetQueryable()
            .Include(x => x.Steps)
            .Where(x => x.EntityType == WorkflowConstants.TargetEntityTypes.LeaveRequest
                     && x.Status == WorkflowStatusCode.Pending)
            .OrderByDescending(x => x.StartedAt)
            .ToListAsync(cancellationToken);

        var matchedIds = await FilterInstancesForUserAsync(pendingInstances, request, cancellationToken);
        if (matchedIds.Count == 0)
            return Array.Empty<ManagerLeavePendingApprovalResponse>();

        var entityIds = matchedIds
            .Select(i => new LeaveRequestId(Guid.Parse(i.EntityId)))
            .Distinct()
            .ToList();

        var leaveRequests = await leaveRequestRepository.GetQueryable()
            .Include(x => x.Employee)
            .Where(x => entityIds.Contains(x.Id))
            .ToListAsync(cancellationToken);

        var employeeIds = leaveRequests
            .Select(x => x.EmployeeId)
            .Distinct()
            .ToList();

        var employeeRows = await employeeRepository.GetQueryable()
            .Include(e => e.PrimaryDepartment)
            .Where(e => employeeIds.Contains(e.Id))
            .ToListAsync(cancellationToken);
        var employees = employeeRows.ToDictionary(e => e.Id);

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

        var results = new List<ManagerLeavePendingApprovalResponse>();
        foreach (var leaveRequest in leaveRequests)
        {
            if (!workflowByEntityId.TryGetValue(leaveRequest.Id.Value, out var workflowInfo))
                continue;

            var (instance, step) = workflowInfo;
            employees.TryGetValue(leaveRequest.EmployeeId, out var employee);

            var stepName = GetStepDisplayName(step);

            string? approverName = null;
            if (step?.ApproverEmployeeId is not null)
                approverNames.TryGetValue(step.ApproverEmployeeId, out approverName);

            results.Add(new ManagerLeavePendingApprovalResponse
            {
                RequestId = leaveRequest.Id.Value.ToString(),
                EmployeeId = leaveRequest.EmployeeId.Value.ToString(),
                EmployeeName = employee?.FullName ?? leaveRequest.Employee?.FullName,
                DepartmentName = employee?.PrimaryDepartment?.Name,
                LeaveTypeCode = leaveRequest.LeaveTypeCode,
                StartDate = leaveRequest.StartDate.ToString("yyyy-MM-dd"),
                EndDate = leaveRequest.EndDate.ToString("yyyy-MM-dd"),
                Days = leaveRequest.RequestedDays,
                Reason = leaveRequest.Reason,
                SubmittedAt = leaveRequest.CreatedAt,
                Status = leaveRequest.StatusCode,
                WorkflowInstanceId = instance.Id.Value.ToString(),
                CurrentStepName = stepName,
                WorkflowStatus = instance.Status,
                CurrentApproverName = approverName
            });
        }

        return results.OrderByDescending(r => r.SubmittedAt).ToList();
    }

    private async Task<List<WorkflowInstance>> FilterInstancesForUserAsync(
        List<WorkflowInstance> instances, GetMyPendingLeaveApprovalsQuery request, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(request.UserId))
            return [];

        var normalizedUserId = request.UserId.Trim();

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
                approvers.Any(a => string.Equals(a.UserId.Value.ToString(), normalizedUserId, StringComparison.OrdinalIgnoreCase)))
            {
                userRoleValues.Add(role);
            }
        }

        var permissionValues = instances
            .Select(i => i.Steps.FirstOrDefault(s => s.Sequence == i.CurrentStep))
            .Where(s => s?.ApproverTypeSnapshot == ApproverType.Permission && s.ApproverValueSnapshot is not null)
            .Select(s => s!.ApproverValueSnapshot)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var userPermissionValues = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var permission in permissionValues)
        {
            if (await permissionResolver.HasPermissionAsync(request.RoleGroups, permission, ct))
                userPermissionValues.Add(permission);
        }

        return instances.Where(instance =>
        {
            var step = instance.Steps.FirstOrDefault(s => s.Sequence == instance.CurrentStep);
            if (step is null) return false;

            return step.ApproverTypeSnapshot switch
            {
                ApproverType.SpecificUser => string.Equals(step.ApproverValueSnapshot, normalizedUserId, StringComparison.OrdinalIgnoreCase),
                ApproverType.DirectManager => string.Equals(step.ApproverUserId, normalizedUserId, StringComparison.OrdinalIgnoreCase),
                ApproverType.DepartmentManager => string.Equals(step.ApproverUserId, normalizedUserId, StringComparison.OrdinalIgnoreCase),
                ApproverType.HrManager => string.Equals(step.ApproverUserId, normalizedUserId, StringComparison.OrdinalIgnoreCase),
                ApproverType.Role => userRoleValues.Contains(step.ApproverValueSnapshot),
                ApproverType.Permission => step.ApproverValueSnapshot is not null && userPermissionValues.Contains(step.ApproverValueSnapshot),
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
