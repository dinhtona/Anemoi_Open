using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Recruitment;
using Anemoi.Hr.ModelIds.ModelIds;
using Microsoft.EntityFrameworkCore;
using OneOf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Anemoi.Hr.Application.Cqrs.Queries.RecruitmentQueries.GetRecruitmentRequestTimeline;

public sealed class GetRecruitmentRequestTimelineHandler(
    ISqlRepository<RecruitmentRequestHistory> historyRepository,
    RecruitmentMapper mapper)
    : IQueryHandler<GetRecruitmentRequestTimelineQuery, OneOf<IReadOnlyCollection<RecruitmentRequestHistoryResponse>, ErrorDetailResponse>>
{
    public async Task<OneOf<IReadOnlyCollection<RecruitmentRequestHistoryResponse>, ErrorDetailResponse>> Handle(
        GetRecruitmentRequestTimelineQuery request,
        CancellationToken cancellationToken)
    {
        var requestId = new RecruitmentRequestId(Guid.Parse(request.RecruitmentRequestId));
        var history = await historyRepository.GetQueryable()
            .Where(x => x.RecruitmentRequestId == requestId)
            .OrderByDescending(x => x.PerformedAt)
            .ToListAsync(cancellationToken);

        return OneOf<IReadOnlyCollection<RecruitmentRequestHistoryResponse>, ErrorDetailResponse>
            .FromT0(mapper.ToHistoryResponses(history));
    }
}
