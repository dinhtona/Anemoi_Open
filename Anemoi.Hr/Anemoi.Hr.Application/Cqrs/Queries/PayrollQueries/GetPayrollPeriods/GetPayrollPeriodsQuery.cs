using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.Hr.Application.Responses;
using System.Collections.Generic;

namespace Anemoi.Hr.Application.Cqrs.Queries.PayrollQueries.GetPayrollPeriods;

public sealed record GetPayrollPeriodsQuery() : IQueryOne<IReadOnlyCollection<PayrollPeriodResponse>>;
