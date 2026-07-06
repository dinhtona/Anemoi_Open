using Anemoi.BuildingBlock.Application.Responses;

namespace Anemoi.Contract.MasterData.Responses;

public sealed class SeedFunctionResponse : ModelResponse
{
    public string SeedServerId { get; set; }
    public string SeedTemplateId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string TablesJson { get; set; }
}
