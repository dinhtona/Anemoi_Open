using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.Hr.Application.Responses;

namespace Anemoi.Hr.Application.Cqrs.Queries.EssQueries.GetMyPayslips;

public sealed record GetMyPayslipsQuery(string UserId, string Email) : IQueryOne<IReadOnlyCollection<EssPayslipResponse>>;
