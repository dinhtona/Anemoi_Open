using Anemoi.BuildingBlock.Domain;
using Anemoi.Contract.MasterData.ModelIds;

namespace Anemoi.MasterData.Domain.Models;

public sealed class SeedFunction : ValueObject
{
    public SeedFunctionId Id { get; set; }
    public SeedServerId SeedServerId { get; set; }
    public SeedServer SeedServer { get; set; }
    public SeedTemplateId SeedTemplateId { get; set; }
    public SeedTemplate SeedTemplate { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string TablesJson { get; set; }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Name;
        yield return SeedServerId;
    }
}
