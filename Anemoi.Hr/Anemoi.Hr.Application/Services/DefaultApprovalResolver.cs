using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Contract.Identity.ModelIds;
using Anemoi.Hr.Application.Abstractions;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Domain.Departments;
using Anemoi.Hr.Domain.Employees;
using Anemoi.Hr.Domain.Workflow;
using Anemoi.Hr.ModelIds.ModelIds;
using Microsoft.EntityFrameworkCore;
using OneOf;

namespace Anemoi.Hr.Application.Services;

public sealed class DefaultApprovalResolver(
    ISqlRepository<Employee> employeeRepository,
    ISqlRepository<Department> departmentRepository,
    IWorkflowRoleResolver roleResolver)
    : IApprovalResolver
{
    public async Task<OneOf<IReadOnlyList<ResolvedApprover>, ErrorDetailResponse>> ResolveApproversAsync(
        string approverType, string? approverValue,
        ApprovalRoutingContext context, CancellationToken ct)
    {
        return approverType switch
        {
            ApproverType.DirectManager => await ResolveDirectManagerAsync(context, ct),
            ApproverType.DepartmentManager => await ResolveDepartmentManagerAsync(context, ct),
            ApproverType.HrManager => await roleResolver.ResolveAsync(WorkflowRole.HrManager, ct),
            ApproverType.SpecificUser => await ResolveSpecificUserAsync(approverValue, ct),
            ApproverType.Role => await roleResolver.ResolveAsync(approverValue ?? WorkflowRole.HrManager, ct),
            _ => HrErrorResponses.Create(HrBusinessErrorCodes.WorkflowApproverNotFound)
        };
    }

    private async Task<OneOf<IReadOnlyList<ResolvedApprover>, ErrorDetailResponse>> ResolveDirectManagerAsync(
        ApprovalRoutingContext context, CancellationToken ct)
    {
        var employee = await employeeRepository.GetQueryable()
            .FirstOrDefaultAsync(e => e.Id == context.RequesterEmployeeId, ct);

        if (employee is null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.WorkflowApproverNotFound);

        if (employee.DirectManagerEmployeeId is null)
            return new List<ResolvedApprover>();

        var manager = await employeeRepository.GetQueryable()
            .FirstOrDefaultAsync(e => e.Id == employee.DirectManagerEmployeeId, ct);

        if (manager is null || manager.IdentityUserId is null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.WorkflowApproverNotFound);

        return new List<ResolvedApprover>
        {
            new(new UserId(manager.IdentityUserId.Value), manager.Id,
                manager.FullName, manager.WorkEmail, "DirectManager")
        };
    }

    private async Task<OneOf<IReadOnlyList<ResolvedApprover>, ErrorDetailResponse>> ResolveDepartmentManagerAsync(
        ApprovalRoutingContext context, CancellationToken ct)
    {
        var employee = await employeeRepository.GetQueryable()
            .FirstOrDefaultAsync(e => e.Id == context.RequesterEmployeeId, ct);

        if (employee is null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.WorkflowApproverNotFound);

        var deptId = context.DepartmentId ?? employee.PrimaryDepartmentId;
        if (deptId is null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.WorkflowApproverNotFound);

        var department = await departmentRepository.GetQueryable()
            .FirstOrDefaultAsync(d => d.Id == deptId, ct);

        if (department is null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.WorkflowApproverNotFound);

        if (department.ManagerEmployeeId is null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.WorkflowApproverNotFound);

        var manager = await employeeRepository.GetQueryable()
            .FirstOrDefaultAsync(e => e.Id == department.ManagerEmployeeId, ct);

        if (manager is null || manager.IdentityUserId is null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.WorkflowApproverNotFound);

        return new List<ResolvedApprover>
        {
            new(new UserId(manager.IdentityUserId.Value), manager.Id,
                manager.FullName, manager.WorkEmail, "DepartmentManager")
        };
    }

    private async Task<OneOf<IReadOnlyList<ResolvedApprover>, ErrorDetailResponse>> ResolveSpecificUserAsync(
        string? approverValue, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(approverValue))
            return HrErrorResponses.Create(HrBusinessErrorCodes.WorkflowApproverNotFound);

        if (Guid.TryParse(approverValue, out var employeeIdGuid))
        {
            var employee = await employeeRepository.GetQueryable()
                .FirstOrDefaultAsync(e => e.Id == new EmployeeId(employeeIdGuid), ct);

            if (employee is not null && employee.IdentityUserId is not null)
            {
                return new List<ResolvedApprover>
                {
                    new(new UserId(employee.IdentityUserId.Value), employee.Id,
                        employee.FullName, employee.WorkEmail, "SpecificUser")
                };
            }
        }

        return HrErrorResponses.Create(HrBusinessErrorCodes.WorkflowApproverNotFound);
    }
}
