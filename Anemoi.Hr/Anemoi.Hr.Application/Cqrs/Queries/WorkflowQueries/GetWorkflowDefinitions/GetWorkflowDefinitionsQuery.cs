using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.Hr.Application.Responses;

namespace Anemoi.Hr.Application.Cqrs.Queries.WorkflowQueries.GetWorkflowDefinitions;

public sealed record GetWorkflowDefinitionsQuery(
    bool? IsActive,
    string? WorkflowTypeCode,
    int Page = 1,
    int PageSize = 20) : IQueryPaged<WorkflowDefinitionResponse>;
