#nullable enable

using System;

namespace Anemoi.Hr.Application.Responses;

public sealed record PayslipDeliveryReportItem
{
    public Guid PayrollRunId { get; init; }
    public Guid EmployeeId { get; init; }
    public string EmployeeCode { get; init; } = default!;
    public string EmployeeName { get; init; } = default!;
    public string PayslipStatus { get; init; } = default!;
    public bool PdfGenerated { get; init; }
    public DateTime? PublishedAt { get; init; }
    public bool EmailSent { get; init; }
    public DateTime? EmailSentAt { get; init; }
    public string? EmailFailureReason { get; init; }
    public bool DownloadAvailable { get; init; }
}
