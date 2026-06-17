using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Recruitment;
using Anemoi.Hr.ModelIds.ModelIds;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Anemoi.Hr.Application.Cqrs.Queries.RecruitmentQueries.GetRecruitmentRequests;

public sealed class GetRecruitmentRequestsHandler(
    ISqlRepository<RecruitmentRequest> requestRepository,
    RecruitmentMapper mapper)
    : IQueryHandler<GetRecruitmentRequestsQuery, PaginationResponse<RecruitmentRequestResponse>>
{
    public async Task<PaginationResponse<RecruitmentRequestResponse>> Handle(
        GetRecruitmentRequestsQuery request,
        CancellationToken cancellationToken)
    {
        var query = requestRepository.GetQueryable()
            .Include(x => x.Department)
            .Include(x => x.Position)
            .AsQueryable();

        if (!string.IsNullOrEmpty(request.Status))
            query = query.Where(x => x.Status == request.Status);
        if (!string.IsNullOrEmpty(request.DepartmentId))
            query = query.Where(x => x.DepartmentId == new DepartmentId(Guid.Parse(request.DepartmentId)));
        if (!string.IsNullOrEmpty(request.PositionId))
            query = query.Where(x => x.PositionId == new PositionId(Guid.Parse(request.PositionId)));
        if (!string.IsNullOrEmpty(request.SearchTerm))
            query = query.Where(x =>
                x.RequestNumber.ToLower().Contains(request.SearchTerm.ToLower()) ||
                x.Reason.ToLower().Contains(request.SearchTerm.ToLower()));

        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize < 1 ? 10 : request.PageSize;

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PaginationResponse<RecruitmentRequestResponse>(
            items.Select(mapper.ToResponse).ToList(),
            total);
    }
}
