using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Application.Cqrs.Queries.DepartmentQueries.GetDepartmentById;

public sealed record GetDepartmentByIdQuery(
    DepartmentId Id) : IQueryOne<DepartmentResponse>;
