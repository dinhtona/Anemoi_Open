using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Application.Cqrs.Queries.WorkflowQueries.GetWorkflowRoleAssignments;

public sealed record GetWorkflowRoleAssignmentsQuery(
    string? Role,
    EmployeeId? EmployeeId,
    int Page = 1,
    int PageSize = 20) : IQueryOne<PaginationResponse<WorkflowRoleAssignmentResponse>>;
