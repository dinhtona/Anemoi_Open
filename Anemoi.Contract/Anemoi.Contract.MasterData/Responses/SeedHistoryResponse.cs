using Anemoi.BuildingBlock.Application.Responses;
using System;

namespace Anemoi.Contract.MasterData.Responses;

public sealed class SeedHistoryResponse : ModelResponse
{
    public string SeedFunctionId { get; set; }
    public DateTime RunAt { get; set; }
    public string ConfigJson { get; set; }
    public string ResultJson { get; set; }
}
