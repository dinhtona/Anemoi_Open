using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Responses;

namespace Anemoi.Hr.Application.Cqrs.Queries.CalendarManagementQueries.WorkingCalendarRule.GetWorkingCalendarRules;

public sealed record GetWorkingCalendarRulesQuery(
    bool? IsActive = null,
    int Page = 1,
    int PageSize = 50,
    string SortBy = null,
    string SortDirection = "asc") : IQuery<PaginationResponse<WorkingCalendarRuleResponse>>;
