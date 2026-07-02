namespace Anemoi.Hr.Domain.EmployeeAssets;

public sealed record AssetType(string Value)
{
    public static readonly AssetType Laptop = new("laptop");
    public static readonly AssetType Phone = new("phone");
    public static readonly AssetType Card = new("card");
    public static readonly AssetType Monitor = new("monitor");
    public static readonly AssetType Equipment = new("equipment");
    public static readonly AssetType Other = new("other");

    private static readonly IReadOnlyCollection<AssetType> All =
    [Laptop, Phone, Card, Monitor, Equipment, Other];

    public static AssetType FromValue(string value) =>
        All.FirstOrDefault(t => t.Value == value) ?? throw new ArgumentException($"Unknown AssetType: {value}");

    public override string ToString() => Value;
}
