using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Contract.Identity.ModelIds;
using Anemoi.Hr.Application.Abstractions;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Domain.Employees;
using Anemoi.Hr.Domain.Workflow;
using Microsoft.EntityFrameworkCore;
using OneOf;

namespace Anemoi.Hr.Application.Services;

public sealed class DefaultWorkflowRoleResolver(
    ISqlRepository<WorkflowRoleAssignment> roleAssignmentRepository,
    ISqlRepository<Employee> employeeRepository)
    : IWorkflowRoleResolver
{
    public async Task<OneOf<IReadOnlyList<ResolvedApprover>, ErrorDetailResponse>> ResolveAsync(
        string workflowRole, CancellationToken ct)
    {
        var assignments = await roleAssignmentRepository.GetQueryable()
            .Where(r => r.Role == workflowRole)
            .ToListAsync(ct);

        if (assignments.Count == 0)
            return HrErrorResponses.Create(HrBusinessErrorCodes.WorkflowApproverNotFound);

        var employeeIds = assignments.Select(a => a.EmployeeId).ToList();
        var employees = await employeeRepository.GetQueryable()
            .Where(e => employeeIds.Contains(e.Id))
            .ToDictionaryAsync(e => e.Id, ct);

        var results = new List<ResolvedApprover>();
        foreach (var assignment in assignments)
        {
            if (!employees.TryGetValue(assignment.EmployeeId, out var employee))
                continue;
            if (employee.IdentityUserId is null)
                continue;

            results.Add(new ResolvedApprover(
                new UserId(employee.IdentityUserId.Value),
                employee.Id,
                employee.FullName,
                employee.WorkEmail,
                $"WorkflowRole:{workflowRole}"));
        }

        if (results.Count == 0)
            return HrErrorResponses.Create(HrBusinessErrorCodes.WorkflowApproverNotFound);

        return results;
    }
}
