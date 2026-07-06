using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.Hr.Application.Cqrs.Common.Dtos;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Application.Cqrs.Queries.SeparationQueries.GetSeparationById;

public sealed record GetSeparationByIdQuery(EmployeeSeparationId Id) : IQueryOne<EmployeeSeparationDto>;
