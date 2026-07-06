using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.CalendarManagement;
using Microsoft.EntityFrameworkCore;

namespace Anemoi.Hr.Application.Cqrs.Queries.CalendarManagementQueries.WorkingCalendarRule.GetWorkingCalendarRules;

public sealed class GetWorkingCalendarRulesHandler(
    ISqlRepository<Domain.CalendarManagement.WorkingCalendarRule> workingCalendarRuleRepository,
    CalendarManagementMapper mapper)
    : IQueryHandler<GetWorkingCalendarRulesQuery, PaginationResponse<WorkingCalendarRuleResponse>>
{
    public async Task<PaginationResponse<WorkingCalendarRuleResponse>> Handle(
        GetWorkingCalendarRulesQuery request,
        CancellationToken cancellationToken)
    {
        var query = workingCalendarRuleRepository.GetQueryable().AsNoTracking();

        if (request.IsActive.HasValue)
            query = query.Where(x => x.IsActive == request.IsActive.Value);

        query = (request.SortBy?.ToLower(), request.SortDirection?.ToLower()) switch
        {
            ("name", "desc") => query.OrderByDescending(x => x.Name),
            ("name", "asc") => query.OrderBy(x => x.Name),
            ("effectivedate", "desc") => query.OrderByDescending(x => x.EffectiveFrom),
            ("effectivedate", "asc") => query.OrderBy(x => x.EffectiveFrom),
            ("createdat", "desc") => query.OrderByDescending(x => x.CreatedAt),
            ("createdat", "asc") => query.OrderBy(x => x.CreatedAt),
            _ => query.OrderBy(x => x.EffectiveFrom)
        };

        var totalRecords = await query.LongCountAsync(cancellationToken);
        var rows = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return new PaginationResponse<WorkingCalendarRuleResponse>(
            rows.Select(mapper.ToWorkingCalendarRuleResponse).ToList(),
            totalRecords);
    }
}
