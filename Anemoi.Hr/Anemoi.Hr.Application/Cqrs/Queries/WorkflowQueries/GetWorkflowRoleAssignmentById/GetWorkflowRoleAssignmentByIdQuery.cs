using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Application.Cqrs.Queries.WorkflowQueries.GetWorkflowRoleAssignmentById;

public sealed record GetWorkflowRoleAssignmentByIdQuery(
    WorkflowRoleAssignmentId Id) : IQueryOne<WorkflowRoleAssignmentResponse>;
