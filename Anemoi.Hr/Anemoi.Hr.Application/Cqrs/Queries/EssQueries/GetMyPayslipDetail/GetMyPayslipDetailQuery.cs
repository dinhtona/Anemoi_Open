using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Application.Cqrs.Queries.EssQueries.GetMyPayslipDetail;

public sealed record GetMyPayslipDetailQuery(string UserId, string Email, PayslipId PayslipId) : IQueryOne<EssPayslipResponse>;
