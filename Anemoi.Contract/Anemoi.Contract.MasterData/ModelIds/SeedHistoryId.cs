using Anemoi.BuildingBlock.Domain;
using System;

namespace Anemoi.Contract.MasterData.ModelIds;

public record SeedHistoryId(Guid Value) : StronglyTypedId<Guid>(Value)
{
    public static SeedHistoryId New() => new(Guid.NewGuid());
}
