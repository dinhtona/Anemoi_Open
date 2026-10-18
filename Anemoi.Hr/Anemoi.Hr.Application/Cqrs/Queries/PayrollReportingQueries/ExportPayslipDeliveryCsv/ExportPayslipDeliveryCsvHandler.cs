#nullable enable

using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Cqrs.Queries.PayrollReportingQueries.Shared;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Employees;
using Anemoi.Hr.Domain.Payroll;
using Microsoft.EntityFrameworkCore;
using OneOf;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Anemoi.Hr.Application.Cqrs.Queries.PayrollReportingQueries.ExportPayslipDeliveryCsv;

public sealed class ExportPayslipDeliveryCsvHandler(
    ISqlRepository<PayrollRun> runRepository,
    ISqlRepository<Employee> employeeRepository,
    ISqlRepository<Payslip> payslipRepository,
    ISqlRepository<PayslipDocument> documentRepository,
    ISqlRepository<PayslipEmailDelivery> emailRepository,
    PayrollReportExportService exportService)
    : IQueryHandler<ExportPayslipDeliveryCsvQuery, OneOf<ExportResult, ErrorDetailResponse>>
{
    public async Task<OneOf<ExportResult, ErrorDetailResponse>> Handle(
        ExportPayslipDeliveryCsvQuery request,
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

        var rows = await PayrollReportingQueryExtensions.BuildPayslipDeliveryReport(
                runRepository.GetQueryable().AsNoTracking(),
                employeeRepository.GetQueryable().AsNoTracking(),
                payslipRepository.GetQueryable().AsNoTracking(),
                documentRepository.GetQueryable().AsNoTracking(),
                emailRepository.GetQueryable().AsNoTracking(),
                filter)
            .Take(ReportExportLimits.MaxRows + 1)
            .ToListAsync(cancellationToken);

        return await exportService.ExportAsync(
            rows,
            ReportTypes.PayslipDelivery,
            new
            {
                request.PayrollPeriodId,
                request.PayrollRunId,
                request.Status,
                request.SortBy,
                request.SortDirection
            },
            cancellationToken);
    }
}
