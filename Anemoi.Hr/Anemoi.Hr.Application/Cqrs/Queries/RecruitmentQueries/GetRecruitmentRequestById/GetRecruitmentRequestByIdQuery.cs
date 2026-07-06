using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.Hr.Application.Responses;

namespace Anemoi.Hr.Application.Cqrs.Queries.RecruitmentQueries.GetRecruitmentRequestById;

public sealed record GetRecruitmentRequestByIdQuery(string Id) : IQueryOne<RecruitmentRequestResponse>;
