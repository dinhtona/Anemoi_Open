namespace Anemoi.Hr.Application.Cqrs.Common.Dtos;

public sealed record ProbationsExpiringDto
{
    public int Count7 { get; set; }
    public int Count14 { get; set; }
    public int Count30 { get; set; }
}

public sealed record NewEmployeesDto
{
    public int Count7 { get; set; }
    public int Count30 { get; set; }
}

public sealed record DashboardLifecycleSummaryDto
{
    public ProbationsExpiringDto ProbationsExpiring { get; set; } = new();
    public int PendingTransfers { get; set; }
    public int PendingSeparations { get; set; }
    public NewEmployeesDto NewEmployees { get; set; } = new();
}
