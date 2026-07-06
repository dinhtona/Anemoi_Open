using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Application.Cqrs.Queries.CompensationQueries.GetAllowanceTypeById;

public sealed record GetAllowanceTypeByIdQuery(AllowanceTypeId Id) : IQueryOne<AllowanceTypeResponse>;
