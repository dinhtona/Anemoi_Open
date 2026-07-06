using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.Hr.Application.Responses;

namespace Anemoi.Hr.Application.Cqrs.Queries.WorkflowQueries.GetWorkflowInstanceById;

public sealed record GetWorkflowInstanceByIdQuery(string Id) : IQueryOne<WorkflowInstanceResponse>;
