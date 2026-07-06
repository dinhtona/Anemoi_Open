using Anemoi.BuildingBlock.Domain;
using Anemoi.Contract.MasterData.ModelIds;
using System;
using System.Collections.Generic;

namespace Anemoi.MasterData.Domain.Models;

public sealed class SeedRowLog : ValueObject
{
    public SeedRowLogId Id { get; set; }
    public SeedServerId SeedServerId { get; set; }
    public SeedServer SeedServer { get; set; }
    public string TableName { get; set; }
    public DateTime RunAt { get; set; }
    public string RowDataJson { get; set; }
    public string PrimaryKeyCondition { get; set; }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Id;
    }
}
