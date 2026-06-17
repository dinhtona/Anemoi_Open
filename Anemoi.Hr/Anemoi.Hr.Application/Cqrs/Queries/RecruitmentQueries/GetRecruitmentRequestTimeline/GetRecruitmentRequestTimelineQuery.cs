using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.Hr.Application.Responses;
using System.Collections.Generic;

namespace Anemoi.Hr.Application.Cqrs.Queries.RecruitmentQueries.GetRecruitmentRequestTimeline;

public sealed record GetRecruitmentRequestTimelineQuery(string RecruitmentRequestId)
    : IQueryOne<IReadOnlyCollection<RecruitmentRequestHistoryResponse>>;
