using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.Hr.Application.Cqrs.Common.Dtos;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Application.Cqrs.Queries.ProbationQueries.GetProbationById;

public sealed record GetProbationByIdQuery(ProbationRecordId Id) : IQueryOne<ProbationRecordDto>;
