namespace Anemoi.Hr.Domain.Recruitment;

public static class InterviewResultCode
{
    public const string Pending = "Pending";
    public const string Passed = "Passed";
    public const string Failed = "Failed";
    public const string NoShow = "NoShow";

    public static readonly string[] CompletedResults = [Passed, Failed, NoShow];

    public static bool IsCompleted(string result) => CompletedResults.Contains(result);
}
