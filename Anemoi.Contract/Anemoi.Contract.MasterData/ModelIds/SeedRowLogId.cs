using Anemoi.BuildingBlock.Domain;
using System;
using Anemoi.BuildingBlock.Application.Helpers;

namespace Anemoi.Contract.MasterData.ModelIds;

public record SeedRowLogId(Guid Value) : StronglyTypedId<Guid>(Value)
{
    public static SeedRowLogId New() => new(IdGenerator.NextGuid());
}
