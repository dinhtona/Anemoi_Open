using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Application.Cqrs.Queries.OvertimeRuleQueries.GetOvertimeRuleById;

public sealed record GetOvertimeRuleByIdQuery(OvertimeRuleId Id) : IQueryOne<OvertimeRuleResponse>;
