using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Recruitment;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Anemoi.Hr.Application.Cqrs.Queries.RecruitmentQueries.GetCandidateSourceEffectiveness;

public sealed class GetCandidateSourceEffectivenessHandler(
    ISqlRepository<Candidate> candidateRepository,
    ISqlRepository<CandidateApplication> applicationRepository)
    : IQueryHandler<GetCandidateSourceEffectivenessQuery, IReadOnlyCollection<CandidateSourceEffectivenessItem>>
{
    public async Task<IReadOnlyCollection<CandidateSourceEffectivenessItem>> Handle(
        GetCandidateSourceEffectivenessQuery request,
        CancellationToken cancellationToken)
    {
        var sources = await candidateRepository.GetQueryable()
            .GroupBy(x => x.Source)
            .Select(g => new { Source = g.Key, CandidateCount = g.Count() })
            .ToListAsync(cancellationToken);

        var result = new List<CandidateSourceEffectivenessItem>();
        foreach (var s in sources)
        {
            var appQuery = applicationRepository.GetQueryable()
                .Where(x => x.Candidate.Source == s.Source);
            if (request.FromDate.HasValue)
                appQuery = appQuery.Where(x => x.AppliedAt >= request.FromDate.Value.ToDateTime(new TimeOnly(0, 0)));
            if (request.ToDate.HasValue)
                appQuery = appQuery.Where(x => x.AppliedAt <= request.ToDate.Value.ToDateTime(new TimeOnly(23, 59)));

            var applicationCount = await appQuery.CountAsync(cancellationToken);
            var hireCount = await appQuery
                .Where(x => x.CurrentStage == CandidateApplicationStageCode.Hired)
                .CountAsync(cancellationToken);

            var conversionCount = await candidateRepository.CountByConditionAsync(
                x => x.Source == s.Source && x.EmployeeId != null, token: cancellationToken);

            var hireRate = applicationCount > 0 ? (double)hireCount / applicationCount : 0;
            var conversionRate = hireCount > 0 ? (double)conversionCount / hireCount : 0;

            result.Add(new CandidateSourceEffectivenessItem(
                s.Source,
                s.CandidateCount,
                applicationCount,
                hireCount,
                (int)conversionCount,
                hireRate,
                conversionRate
            ));
        }

        return result;
    }
}
