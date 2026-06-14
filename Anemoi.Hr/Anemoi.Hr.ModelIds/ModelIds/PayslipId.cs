using Anemoi.BuildingBlock.Domain;
using System;

namespace Anemoi.Hr.ModelIds.ModelIds;

public sealed record PayslipId(Guid Value) : StronglyTypedId<Guid>(Value);
