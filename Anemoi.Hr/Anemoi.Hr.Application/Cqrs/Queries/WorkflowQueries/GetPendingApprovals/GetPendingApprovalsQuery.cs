using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.Hr.Application.Responses;

namespace Anemoi.Hr.Application.Cqrs.Queries.WorkflowQueries.GetPendingApprovals;

public sealed record GetPendingApprovalsQuery(
    string? UserId = null,
    int Page = 1,
    int PageSize = 20) : IQueryPaged<WorkflowInstanceResponse>;
