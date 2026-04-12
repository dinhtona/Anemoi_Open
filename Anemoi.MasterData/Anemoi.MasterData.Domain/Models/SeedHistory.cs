using Anemoi.BuildingBlock.Domain;
using Anemoi.Contract.MasterData.ModelIds;
using System;
using System.Collections.Generic;

namespace Anemoi.MasterData.Domain.Models;

public sealed class SeedHistory : ValueObject
{
    public SeedHistoryId Id { get; set; }
    public SeedFunctionId SeedFunctionId { get; set; }
    public SeedFunction SeedFunction { get; set; }
    public DateTime RunAt { get; set; }
    public string ConfigJson { get; set; }
    public string ResultJson { get; set; }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Id;
    }
}
