namespace Anemoi.Hr.Application.Responses;

public sealed class OnboardingDashboardResponse
{
    public int TotalActiveOnboardings { get; set; }
    public int TotalOverdueTasks { get; set; }
    public int TotalPendingTasks { get; set; }
    public int TotalUpcomingTasks { get; set; }
    public int CompletedThisMonth { get; set; }
    public double AverageCompletionRate { get; set; }
}
