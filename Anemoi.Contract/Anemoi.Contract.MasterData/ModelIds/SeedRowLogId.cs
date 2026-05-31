using Anemoi.BuildingBlock.Domain;
using System;

namespace Anemoi.Contract.MasterData.ModelIds;

public record SeedRowLogId(Guid Value) : StronglyTypedId<Guid>(Value)
{
}
