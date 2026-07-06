using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Application.Cqrs.Queries.RecruitmentQueries.GetCandidateById;

public sealed record GetCandidateByIdQuery(CandidateId Id)
    : IQueryOne<CandidateResponse>;
