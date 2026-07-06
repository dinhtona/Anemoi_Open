#nullable enable

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Cqrs.Queries.PayrollReportingQueries.Shared;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Employees;
using Anemoi.Hr.Domain.Payroll;
using Microsoft.EntityFrameworkCore;

namespace Anemoi.Hr.Application.Cqrs.Queries.PayrollReportingQueries.GetPayslipDeliveryReport;

public sealed class GetPayslipDeliveryReportHandler(
    ISqlRepository<PayrollRun> runRepository,
    ISqlRepository<Employee> employeeRepository,
    ISqlRepository<Payslip> payslipRepository,
    ISqlRepository<PayslipDocument> documentRepository,
    ISqlRepository<PayslipEmailDelivery> emailRepository)
    : IQueryHandler<GetPayslipDeliveryReportQuery, PaginationResponse<PayslipDeliveryReportItem>>
{
    public async Task<PaginationResponse<PayslipDeliveryReportItem>> Handle(
        GetPayslipDeliveryReportQuery request,
        CancellationToken cancellationToken)
    {
        var filter = new PayrollReportingFilter(
            request.PayrollPeriodId,
            request.PayrollRunId,
            null,
            request.Status,
            null,
            null,
            request.SortBy,
            request.SortDirection);

        var query = PayrollReportingQueryExtensions.BuildPayslipDeliveryReport(
            runRepository.GetQueryable().AsNoTracking(),
            employeeRepository.GetQueryable().AsNoTracking(),
            payslipRepository.GetQueryable().AsNoTracking(),
            documentRepository.GetQueryable().AsNoTracking(),
            emailRepository.GetQueryable().AsNoTracking(),
            filter);

        var totalRecords = await query.LongCountAsync(cancellationToken);
        var items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return new PaginationResponse<PayslipDeliveryReportItem>(items, totalRecords);
    }
}
