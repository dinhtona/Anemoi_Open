using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.Hr.Application.Responses;
using System.Collections.Generic;

namespace Anemoi.Hr.Application.Cqrs.Queries.CompensationQueries.GetAllowanceTypes;

public sealed record GetAllowanceTypesQuery : IQuery<IReadOnlyCollection<AllowanceTypeResponse>>;
