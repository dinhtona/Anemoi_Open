using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Recruitment;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Anemoi.Hr.Application.Cqrs.Queries.RecruitmentQueries.GetTimeToHire;

public sealed class GetTimeToHireHandler(
    ISqlRepository<CandidateApplication> applicationRepository,
    ISqlRepository<HiringDecision> decisionRepository)
    : IQueryHandler<GetTimeToHireQuery, TimeToHireResponse>
{
    public async Task<TimeToHireResponse> Handle(
        GetTimeToHireQuery request,
        CancellationToken cancellationToken)
    {
        var hireDecisionsQuery = decisionRepository.GetQueryable()
            .Where(x => x.Decision == HiringDecisionCode.Hire);

        if (request.FromDate.HasValue)
            hireDecisionsQuery = hireDecisionsQuery.Where(x => x.DecidedAt >= request.FromDate.Value.ToDateTime(new TimeOnly(0, 0)));
        if (request.ToDate.HasValue)
            hireDecisionsQuery = hireDecisionsQuery.Where(x => x.DecidedAt <= request.ToDate.Value.ToDateTime(new TimeOnly(23, 59)));

        var hireDecisions = await hireDecisionsQuery
            .Include(x => x.CandidateApplication)
            .ToListAsync(cancellationToken);

        var daysToHire = hireDecisions
            .Where(x => x.CandidateApplication != null)
            .Select(x => (x.DecidedAt.Date - x.CandidateApplication.AppliedAt.Date).Days)
            .Where(d => d >= 0)
            .ToList();

        if (daysToHire.Count == 0)
            return new TimeToHireResponse(0, null, 0, 0, 0);

        var sorted = daysToHire.OrderBy(d => d).ToList();
        var median = sorted.Count % 2 == 0
            ? (sorted[sorted.Count / 2 - 1] + sorted[sorted.Count / 2]) / 2.0
            : sorted[sorted.Count / 2];

        return new TimeToHireResponse(
            daysToHire.Average(),
            median,
            sorted.Min(),
            sorted.Max(),
            sorted.Count
        );
    }
}
