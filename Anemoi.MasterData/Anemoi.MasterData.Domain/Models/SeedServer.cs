using Anemoi.BuildingBlock.Domain;
using Anemoi.Contract.MasterData.ModelIds;

namespace Anemoi.MasterData.Domain.Models;

public sealed class SeedServer : ValueObject
{
    public SeedServerId Id { get; set; }
    public string Name { get; set; }
    public string ConnectionString { get; set; }
    public string Environment { get; set; }
    public string Provider { get; set; } = "SqlServer";
    public List<SeedFunction> SeedFunctions { get; set; }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Name;
        yield return Environment;
    }
}
