using System;
using Anemoi.BuildingBlock.Domain;

namespace Anemoi.Centralize.Domain.ModelIds;

public sealed record MockRouteId(Guid Value) : StronglyTypedId<Guid>(Value);
