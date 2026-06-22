namespace Anemoi.Hr.Domain.Employees;

public static class EmploymentStatusCode
{
    public const string Draft = "Draft";
    public const string PendingOnboarding = "PendingOnboarding";
    public const string Onboarding = "Onboarding";
    public const string Active = "Active";
    public const string Suspended = "Suspended";
    public const string Resigned = "Resigned";
    public const string Terminated = "Terminated";
    public const string Archived = "Archived";

    public static readonly Dictionary<string, HashSet<string>> ValidTransitions = new()
    {
        [Draft] = { PendingOnboarding },
        [PendingOnboarding] = { Onboarding },
        [Onboarding] = { Active },
        [Active] = { Suspended, Resigned, Terminated },
        [Suspended] = { Active },
        [Resigned] = { Archived },
        [Terminated] = { Archived },
        [Archived] = { }
    };

    public static bool IsValidTransition(string from, string to) =>
        ValidTransitions.TryGetValue(from, out var next) && next.Contains(to);
}
