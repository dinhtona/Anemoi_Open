using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.Hr.Application.Responses;

namespace Anemoi.Hr.Application.Cqrs.Queries.WorkflowQueries.GetWorkflowDefinitionById;

public sealed record GetWorkflowDefinitionByIdQuery(string Id) : IQueryOne<WorkflowDefinitionResponse>;
