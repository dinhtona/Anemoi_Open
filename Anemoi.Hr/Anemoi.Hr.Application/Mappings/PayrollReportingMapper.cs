#nullable enable

using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Application.Cqrs.Queries.PayrollReportingQueries.Shared;
using Riok.Mapperly.Abstractions;

namespace Anemoi.Hr.Application.Mappings;

[Mapper]
public partial class PayrollReportingMapper
{
    public partial PayrollRunSummaryReportItem ToReportItem(PayrollRunSummaryProjection source);
    public partial PayrollItemDetailReportItem ToReportItem(PayrollItemDetailProjection source);
    public partial PayslipSummaryReportItem ToReportItem(PayslipSummaryProjection source);
}
