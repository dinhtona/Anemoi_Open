using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;
using System.Collections.Generic;

namespace Anemoi.Hr.Application.Cqrs.Queries.PayslipQueries.GetPayslipsByPayrollRun;

public sealed record GetPayslipsByPayrollRunQuery(PayrollRunId PayrollRunId) : IQueryOne<IReadOnlyCollection<PayslipResponse>>;
