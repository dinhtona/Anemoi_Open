using Anemoi.BuildingBlock.Domain;
using System;

namespace Anemoi.Hr.ModelIds.ModelIds;

public sealed record PayrollRunId(Guid Value) : StronglyTypedId<Guid>(Value);
