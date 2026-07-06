using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.BuildingBlock.Application.Queries;
using Anemoi.Hr.Application.Responses;

namespace Anemoi.Hr.Application.Cqrs.Queries.OvertimeRuleQueries.GetOvertimeRules;

public sealed record GetOvertimeRulesQuery(string? SearchKey, bool? IsActive)
    : GetManyQuery, IQueryPaged<OvertimeRuleResponse>;
