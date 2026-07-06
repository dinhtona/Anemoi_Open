using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.Hr.Application.Responses;

namespace Anemoi.Hr.Application.Cqrs.Queries.WorkflowQueries.GetWorkflowInstances;

public sealed record GetWorkflowInstancesQuery(
    string? Status,
    string? EntityType,
    string? WorkflowDefinitionId,
    int Page = 1,
    int PageSize = 20) : IQueryPaged<WorkflowInstanceResponse>;
