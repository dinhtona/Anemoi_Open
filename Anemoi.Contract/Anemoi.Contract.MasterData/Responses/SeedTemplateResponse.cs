using Anemoi.BuildingBlock.Application.Responses;

namespace Anemoi.Contract.MasterData.Responses;

public sealed class SeedTemplateResponse : ModelResponse
{
    public string Name { get; set; }
    public string Description { get; set; }
    public string ConfigJson { get; set; }
}
