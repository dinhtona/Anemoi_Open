using Anemoi.BuildingBlock.Domain;
using System;

namespace Anemoi.Hr.ModelIds.ModelIds;

public sealed record PayrollPeriodId(Guid Value) : StronglyTypedId<Guid>(Value);
