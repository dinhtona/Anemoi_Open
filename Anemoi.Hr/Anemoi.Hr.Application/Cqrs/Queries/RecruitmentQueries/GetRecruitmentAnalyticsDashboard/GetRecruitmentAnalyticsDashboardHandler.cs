using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Cqrs.Queries.RecruitmentQueries.GetApplicationsByStage;
using Anemoi.Hr.Application.Cqrs.Queries.RecruitmentQueries.GetCandidateSourceEffectiveness;
using Anemoi.Hr.Application.Cqrs.Queries.RecruitmentQueries.GetHiringByDepartment;
using Anemoi.Hr.Application.Cqrs.Queries.RecruitmentQueries.GetRecruitmentOverview;
using Anemoi.Hr.Application.Cqrs.Queries.RecruitmentQueries.GetTimeToHire;
using Anemoi.Hr.Application.Responses;
using MediatR;
using OneOf;
using System.Threading;
using System.Threading.Tasks;

namespace Anemoi.Hr.Application.Cqrs.Queries.RecruitmentQueries.GetRecruitmentAnalyticsDashboard;

public sealed class GetRecruitmentAnalyticsDashboardHandler(ISender sender)
    : IQueryHandler<GetRecruitmentAnalyticsDashboardQuery, OneOf<RecruitmentAnalyticsDashboardResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<RecruitmentAnalyticsDashboardResponse, ErrorDetailResponse>> Handle(
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

        if (overview.IsT1) return overview.AsT1;
        if (byStage.IsT1) return byStage.AsT1;
        if (byDept.IsT1) return byDept.AsT1;
        if (sourceEffectiveness.IsT1) return sourceEffectiveness.AsT1;
        if (timeToHire.IsT1) return timeToHire.AsT1;

        return new RecruitmentAnalyticsDashboardResponse(
            overview.AsT0, byStage.AsT0, byDept.AsT0, sourceEffectiveness.AsT0, timeToHire.AsT0);
    }
}
