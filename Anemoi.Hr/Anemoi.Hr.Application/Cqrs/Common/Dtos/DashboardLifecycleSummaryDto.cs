namespace Anemoi.Hr.Application.Cqrs.Common.Dtos;

public sealed record DashboardLifecycleSummaryDto
{
    public int ExpiringProbationCount { get; set; }
    public int PendingTransferCount { get; set; }
    public int PendingSeparationCount { get; set; }
    public int NewEmployeeCount { get; set; }
    public int ActiveProbationCount { get; set; }
    public int ActiveTransferCount { get; set; }
    public int ActiveSeparationCount { get; set; }
}
