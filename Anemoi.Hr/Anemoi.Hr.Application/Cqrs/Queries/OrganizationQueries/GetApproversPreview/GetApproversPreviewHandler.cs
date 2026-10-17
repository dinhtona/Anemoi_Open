using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Abstractions;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Models;
using Anemoi.Hr.Domain.Employees;
using Anemoi.Hr.Domain.Workflow;
using Microsoft.EntityFrameworkCore;
using OneOf;

namespace Anemoi.Hr.Application.Cqrs.Queries.OrganizationQueries.GetApproversPreview;

public sealed class GetApproversPreviewHandler(
    ISqlRepository<Employee> employeeRepository,
    IApprovalResolver approvalResolver,
    IWorkflowHierarchyResolver hierarchyResolver)
    : IQueryHandler<GetApproversPreviewQuery, OneOf<List<WorkflowApproverCandidate>, ErrorDetailResponse>>
{
    public async Task<OneOf<List<WorkflowApproverCandidate>, ErrorDetailResponse>> Handle(
        GetApproversPreviewQuery request, CancellationToken cancellationToken)
    {
        var employee = await employeeRepository.GetQueryable()
            .Include(e => e.PrimaryDepartment)
            .FirstOrDefaultAsync(e => e.Id == request.EmployeeId, cancellationToken);
        if (employee is null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.EmployeeNotFound);

        var context = new ApprovalRoutingContext(
            request.EmployeeId, employee.PrimaryDepartmentId, null, request.EntityType, null);

        var candidates = new List<WorkflowApproverCandidate>();

        var hierarchy = await hierarchyResolver.ResolveHierarchyAsync(request.EmployeeId, cancellationToken);
        var maxSteps = WorkflowConstants.DefaultPolicy.GetStepCount(request.EntityType);
        var selected = hierarchy.Take(maxSteps > 0 ? maxSteps : 10).ToList();

        foreach (var step in selected)
        {
            var result = await approvalResolver.ResolveApproversAsync(
                step.ApproverType, step.ApproverValue, context, cancellationToken);

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
}
