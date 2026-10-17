#nullable enable

using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Cqrs.Queries.PayrollReportingQueries.Shared;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Insurance;
using Anemoi.Hr.Domain.Payroll;
using Anemoi.Hr.Domain.Taxation;
using Microsoft.EntityFrameworkCore;
using OneOf;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Anemoi.Hr.Application.Cqrs.Queries.PayrollReportingQueries.GetPayrollVarianceReport;

public sealed class GetPayrollVarianceReportHandler(
    ISqlRepository<PayrollRun> runRepository,
    ISqlRepository<PayrollPeriod> periodRepository,
    ISqlRepository<TaxCalculationSnapshot> taxRepository,
    ISqlRepository<InsuranceCalculationSnapshot> insuranceRepository)
    : IQueryHandler<GetPayrollVarianceReportQuery, OneOf<PaginationResponse<PayrollVarianceReportItem>, ErrorDetailResponse>>
{
    public async Task<OneOf<PaginationResponse<PayrollVarianceReportItem>, ErrorDetailResponse>> Handle(
        GetPayrollVarianceReportQuery request,
        CancellationToken cancellationToken)
    {
        var query = PayrollReportingQueryExtensions.BuildPayrollVariance(
            runRepository.GetQueryable().AsNoTracking(),
            periodRepository.GetQueryable().AsNoTracking(),
            taxRepository.GetQueryable().AsNoTracking(),
            insuranceRepository.GetQueryable().AsNoTracking(),
            request.CurrentPayrollPeriodId,
            request.PreviousPayrollPeriodId,
            request.CurrentPayrollRunId,
            request.PreviousPayrollRunId,
            request.IncludeRemovedEmployees,
            request.SortBy,
            request.SortDirection);

        var totalRecords = await query.LongCountAsync(cancellationToken);
        var items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return new PaginationResponse<PayrollVarianceReportItem>(items, totalRecords);
    }
}
