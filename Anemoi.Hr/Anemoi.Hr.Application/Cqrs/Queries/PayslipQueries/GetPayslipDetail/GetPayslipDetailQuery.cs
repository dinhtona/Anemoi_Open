using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Application.Cqrs.Queries.PayslipQueries.GetPayslipDetail;

public sealed record GetPayslipDetailQuery(PayslipId PayslipId) : IQueryOne<PayslipResponse>;
