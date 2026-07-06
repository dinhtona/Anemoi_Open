using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;
using System.Collections.Generic;

namespace Anemoi.Hr.Application.Cqrs.Queries.PayrollQueries.GetPayrollRuns;

public sealed record GetPayrollRunsQuery(PayrollPeriodId PayrollPeriodId)
    : IQueryOne<IReadOnlyCollection<PayrollRunResponse>>;
