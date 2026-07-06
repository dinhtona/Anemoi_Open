using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.Hr.Application.Cqrs.Queries.RecruitmentQueries.GetApplicationsByStage;
using Anemoi.Hr.Application.Cqrs.Queries.RecruitmentQueries.GetCandidateSourceEffectiveness;
using Anemoi.Hr.Application.Cqrs.Queries.RecruitmentQueries.GetHiringByDepartment;
using Anemoi.Hr.Application.Cqrs.Queries.RecruitmentQueries.GetRecruitmentOverview;
using Anemoi.Hr.Application.Cqrs.Queries.RecruitmentQueries.GetTimeToHire;
using Anemoi.Hr.Application.Responses;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Anemoi.Hr.Application.Cqrs.Queries.RecruitmentQueries.GetRecruitmentAnalyticsDashboard;

public sealed class GetRecruitmentAnalyticsDashboardHandler(ISender sender)
    : IQueryHandler<GetRecruitmentAnalyticsDashboardQuery, RecruitmentAnalyticsDashboardResponse>
{
    public async Task<RecruitmentAnalyticsDashboardResponse> Handle(
        GetRecruitmentAnalyticsDashboardQuery request,
        CancellationToken cancellationToken)
    {
        var overview = await sender.Send(
            new GetRecruitmentOverviewQuery(request.FromDate, request.ToDate), cancellationToken);
        var byStage = await sender.Send(
            new GetApplicationsByStageQuery(request.FromDate, request.ToDate), cancellationToken);
        var byDept = await sender.Send(
            new GetHiringByDepartmentQuery(request.FromDate, request.ToDate), cancellationToken);
        var sourceEffectiveness = await sender.Send(
            new GetCandidateSourceEffectivenessQuery(request.FromDate, request.ToDate), cancellationToken);
        var timeToHire = await sender.Send(
            new GetTimeToHireQuery(request.FromDate, request.ToDate), cancellationToken);

        return new RecruitmentAnalyticsDashboardResponse(
            overview, byStage, byDept, sourceEffectiveness, timeToHire);
    }
}
