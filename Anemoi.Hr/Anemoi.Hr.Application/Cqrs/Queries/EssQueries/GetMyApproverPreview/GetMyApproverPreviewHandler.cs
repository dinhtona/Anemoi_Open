using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Abstractions;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Employees;
using Anemoi.Hr.Domain.Workflow;
using Anemoi.Hr.ModelIds.ModelIds;
using Microsoft.EntityFrameworkCore;
using OneOf;

namespace Anemoi.Hr.Application.Cqrs.Queries.EssQueries.GetMyApproverPreview;

public sealed class GetMyApproverPreviewHandler(
    ISqlRepository<Employee> employeeRepository,
    IWorkflowBuilder workflowBuilder,
    IApprovalResolver approvalResolver)
    : IQueryHandler<GetMyApproverPreviewQuery, OneOf<EssApproverPreviewResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<EssApproverPreviewResponse, ErrorDetailResponse>> Handle(
        GetMyApproverPreviewQuery request, CancellationToken cancellationToken)
    {
        Employee employee = null;
        if (Guid.TryParse(request.UserId, out var identityUserId))
            employee = await employeeRepository.GetQueryable()
                .Include(e => e.PrimaryPosition)
                .FirstOrDefaultAsync(e => e.IdentityUserId == identityUserId, cancellationToken);

        if (employee is null && !string.IsNullOrEmpty(request.Email))
        {
            var searchEmail = request.Email.ToLower();
            employee = await employeeRepository.GetQueryable()
                .Include(e => e.PrimaryPosition)
                .FirstOrDefaultAsync(e => e.WorkEmail != null && e.WorkEmail.ToLower() == searchEmail, cancellationToken);
        }

        if (employee is null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.EmployeeNotFound);

        var buildResult = await workflowBuilder.BuildAsync(
            WorkflowConstants.TargetEntityTypes.LeaveRequest,
            employee.Id, request.UserId, cancellationToken);

        if (buildResult.TryPickT1(out var buildError, out var result))
            return new EssApproverPreviewResponse { CanResolve = false };

        var firstStep = result.Steps.FirstOrDefault();
        if (firstStep is null)
            return new EssApproverPreviewResponse { CanResolve = false };

        var context = new ApprovalRoutingContext(
            employee.Id, employee.PrimaryDepartmentId,
            employee.PrimaryPositionId,
            WorkflowConstants.TargetEntityTypes.LeaveRequest, null);

        var resolveResult = await approvalResolver.ResolveApproversAsync(
            firstStep.ApproverTypeSnapshot,
            firstStep.ApproverValueSnapshot,
            context, cancellationToken);

        if (resolveResult.TryPickT1(out _, out var approvers) || approvers.Count == 0)
            return new EssApproverPreviewResponse { CanResolve = false };

        var approver = approvers[0];
        return new EssApproverPreviewResponse
        {
            EmployeeId = approver.EmployeeId.Value.ToString(),
            FullName = approver.FullName,
            PositionName = null,
            ResolutionSource = approver.ResolutionSource,
            CanResolve = true
        };
    }
}
