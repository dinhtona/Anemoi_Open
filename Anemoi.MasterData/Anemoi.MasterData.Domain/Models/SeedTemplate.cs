using Anemoi.BuildingBlock.Domain;
using Anemoi.Contract.MasterData.ModelIds;

namespace Anemoi.MasterData.Domain.Models;

public sealed class SeedTemplate : ValueObject
{
    public SeedTemplateId Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string ConfigJson { get; set; }
    public List<SeedFunction> RelatedFunctions { get; set; }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Name;
    }
}
