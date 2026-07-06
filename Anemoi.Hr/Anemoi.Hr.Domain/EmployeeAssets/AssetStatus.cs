namespace Anemoi.Hr.Domain.EmployeeAssets;

public sealed record AssetStatus(string Value)
{
    public static readonly AssetStatus Available = new("available");
    public static readonly AssetStatus Assigned = new("assigned");
    public static readonly AssetStatus Returned = new("returned");
    public static readonly AssetStatus Lost = new("lost");
    public static readonly AssetStatus Damaged = new("damaged");

    private static readonly IReadOnlyCollection<AssetStatus> All =
    [Available, Assigned, Returned, Lost, Damaged];

    public static AssetStatus FromValue(string value) =>
        All.FirstOrDefault(s => s.Value == value) ?? throw new ArgumentException($"Unknown AssetStatus: {value}");

    public bool IsTerminal => this == Lost || this == Damaged;

    public override string ToString() => Value;
}
