namespace Anemoi.Hr.Domain.Recruitment;

public static class CandidateApplicationStageCode
{
    public const string Applied = "Applied";
    public const string Screening = "Screening";
    public const string Interview = "Interview";
    public const string Offer = "Offer";
    public const string Hired = "Hired";
    public const string Rejected = "Rejected";
    public const string Withdrawn = "Withdrawn";

    public static readonly string[] TerminalStages = [Hired, Rejected, Withdrawn];

    public static bool IsTerminal(string stage) => TerminalStages.Contains(stage);

    public static readonly HashSet<(string From, string To)> ValidTransitions = new()
    {
        (Applied, Screening),
        (Screening, Interview),
        (Interview, Offer),
        (Offer, Hired),
        (Applied, Rejected),
        (Screening, Rejected),
        (Interview, Rejected),
        (Offer, Rejected),
        (Applied, Withdrawn),
        (Screening, Withdrawn),
        (Interview, Withdrawn),
        (Offer, Withdrawn),
    };

    public static bool IsValidTransition(string fromStage, string toStage)
    {
        if (fromStage == null && toStage == Applied)
            return true;
        return ValidTransitions.Contains((fromStage, toStage));
    }
}
