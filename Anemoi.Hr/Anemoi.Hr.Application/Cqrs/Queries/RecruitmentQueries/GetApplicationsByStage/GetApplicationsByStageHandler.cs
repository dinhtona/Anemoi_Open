using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Recruitment;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Anemoi.Hr.Application.Cqrs.Queries.RecruitmentQueries.GetApplicationsByStage;

public sealed class GetApplicationsByStageHandler(
    ISqlRepository<CandidateApplication> applicationRepository)
    : IQueryHandler<GetApplicationsByStageQuery, IReadOnlyCollection<ApplicationsByStageItem>>
{
    public async Task<IReadOnlyCollection<ApplicationsByStageItem>> Handle(
        GetApplicationsByStageQuery request,
        CancellationToken cancellationToken)
    {
        var query = applicationRepository.GetQueryable();

        if (request.FromDate.HasValue)
            query = query.Where(x => x.AppliedAt >= request.FromDate.Value.ToDateTime(new TimeOnly(0, 0)));
        if (request.ToDate.HasValue)
            query = query.Where(x => x.AppliedAt <= request.ToDate.Value.ToDateTime(new TimeOnly(23, 59)));

        var groups = await query
            .GroupBy(x => x.CurrentStage)
            .Select(g => new { Stage = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        var stageOrder = new[]
        {
            CandidateApplicationStageCode.Applied,
            CandidateApplicationStageCode.Screening,
            CandidateApplicationStageCode.Interview,
            CandidateApplicationStageCode.Offer,
            CandidateApplicationStageCode.Hired,
            CandidateApplicationStageCode.Rejected,
            CandidateApplicationStageCode.Withdrawn
        };

        return stageOrder
            .Where(s => groups.Any(g => g.Stage == s))
            .Select(s => new ApplicationsByStageItem(s,
                groups.FirstOrDefault(g => g.Stage == s)?.Count ?? 0))
            .ToList();
    }
}
