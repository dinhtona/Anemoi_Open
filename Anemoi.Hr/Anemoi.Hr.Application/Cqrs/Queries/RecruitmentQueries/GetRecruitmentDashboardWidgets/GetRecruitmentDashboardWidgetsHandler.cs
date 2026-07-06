using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Recruitment;
using Microsoft.EntityFrameworkCore;
using OneOf;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Anemoi.Hr.Application.Cqrs.Queries.RecruitmentQueries.GetRecruitmentDashboardWidgets;

public sealed class GetRecruitmentDashboardWidgetsHandler(
    ISqlRepository<RecruitmentRequest> requestRepository,
    ISqlRepository<RecruitmentOpening> openingRepository)
    : IQueryHandler<GetRecruitmentDashboardWidgetsQuery, OneOf<RecruitmentDashboardWidgetsResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<RecruitmentDashboardWidgetsResponse, ErrorDetailResponse>> Handle(
        GetRecruitmentDashboardWidgetsQuery request,
        CancellationToken cancellationToken)
    {
        var openRequests = await requestRepository.GetQueryable()
            .CountAsync(x => x.Status == RecruitmentRequestStatusCode.Draft ||
                             x.Status == RecruitmentRequestStatusCode.Submitted, cancellationToken);
        var approvedRequests = await requestRepository.GetQueryable()
            .CountAsync(x => x.Status == RecruitmentRequestStatusCode.Approved, cancellationToken);
        var rejectedRequests = await requestRepository.GetQueryable()
            .CountAsync(x => x.Status == RecruitmentRequestStatusCode.Rejected, cancellationToken);
        var pendingApprovals = await requestRepository.GetQueryable()
            .CountAsync(x => x.Status == RecruitmentRequestStatusCode.Submitted, cancellationToken);

        var openings = await openingRepository.GetQueryable()
            .ToListAsync(cancellationToken);
        var openPositions = openings.Count(x => x.Status == "Open");
        var vacancies = openings.Sum(x => x.RemainingHeadcount);

        return new RecruitmentDashboardWidgetsResponse(
            openRequests, approvedRequests, rejectedRequests,
            pendingApprovals, openPositions, vacancies,
            0, 0, 0, 0);
    }
}
