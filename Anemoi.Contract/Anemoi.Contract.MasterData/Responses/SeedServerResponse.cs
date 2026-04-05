using Anemoi.BuildingBlock.Application.Responses;

namespace Anemoi.Contract.MasterData.Responses;

public sealed class SeedServerResponse : ModelResponse
{
    public string Name { get; set; }
    public string ConnectionString { get; set; }
    public string Environment { get; set; }
    public string Provider { get; set; }
}
