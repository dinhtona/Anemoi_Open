using Anemoi.BuildingBlock.Domain;
using System;

namespace Anemoi.Hr.ModelIds.ModelIds;

public sealed record TaxRuleSetId(Guid Value) : StronglyTypedId<Guid>(Value);
public sealed record TaxBracketId(Guid Value) : StronglyTypedId<Guid>(Value);
public sealed record TaxDeductionRuleId(Guid Value) : StronglyTypedId<Guid>(Value);
public sealed record TaxCalculationSnapshotId(Guid Value) : StronglyTypedId<Guid>(Value);
