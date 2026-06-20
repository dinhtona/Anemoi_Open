using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Workflow;
using Microsoft.EntityFrameworkCore;

namespace Anemoi.Hr.Application.Cqrs.Queries.WorkflowQueries.GetWorkflowRoleAssignments;

public sealed class GetWorkflowRoleAssignmentsHandler(
    ISqlRepository<WorkflowRoleAssignment> repository)
    : IQueryHandler<GetWorkflowRoleAssignmentsQuery, PaginationResponse<WorkflowRoleAssignmentResponse>>
{
    public async Task<PaginationResponse<WorkflowRoleAssignmentResponse>> Handle(
        GetWorkflowRoleAssignmentsQuery request, CancellationToken cancellationToken)
    {
        var baseQuery = repository.GetQueryable().AsQueryable();

        if (!string.IsNullOrEmpty(request.Role))
            baseQuery = baseQuery.Where(x => x.Role == request.Role);
        if (request.EmployeeId is not null)
            baseQuery = baseQuery.Where(x => x.EmployeeId == request.EmployeeId);

        var total = await baseQuery.CountAsync(cancellationToken);
        var items = await baseQuery
            .OrderByDescending(x => x.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(x => new WorkflowRoleAssignmentResponse(
                x.Id, x.Role, x.EmployeeId,
                x.Employee.FullName, x.Employee.EmployeeCode,
                x.Employee.PrimaryDepartment.Name,
                x.Employee.PrimaryPosition.Name,
                x.CreatedAt
            ))
            .ToListAsync(cancellationToken);

        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize < 1 ? 10 : request.PageSize;

        return new PaginationResponse<WorkflowRoleAssignmentResponse>(items, total);
    }
}
