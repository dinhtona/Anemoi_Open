using Anemoi.BuildingBlock.Application.Responses;
using System;
using System.Collections.Generic;

namespace Anemoi.Contract.MasterData.Responses;

public sealed class SeedRowLogResponse : ModelResponse
{
    public string SeedServerId { get; set; }
    public string TableName { get; set; }
    public DateTime RunAt { get; set; }
    public Dictionary<string, object> Data { get; set; }
    public string PrimaryKeyCondition { get; set; }
}
