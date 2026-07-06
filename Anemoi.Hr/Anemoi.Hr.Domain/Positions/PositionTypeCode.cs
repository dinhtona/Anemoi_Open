namespace Anemoi.Hr.Domain.Positions;

public static class PositionTypeCode
{
    public const string Manager = "manager";
    public const string IndividualContributor = "individual_contributor";
    public const string Administrator = "administrator";

    public static readonly string[] All = [Manager, IndividualContributor, Administrator];

    public static bool IsValid(string value) => All.Contains(value);
}
