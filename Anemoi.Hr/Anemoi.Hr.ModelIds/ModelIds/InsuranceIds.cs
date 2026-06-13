using Anemoi.BuildingBlock.Domain;
using System;

namespace Anemoi.Hr.ModelIds.ModelIds;

public sealed record InsuranceRuleSetId(Guid Value) : StronglyTypedId<Guid>(Value);
public sealed record InsuranceContributionRuleId(Guid Value) : StronglyTypedId<Guid>(Value);
public sealed record InsuranceCalculationSnapshotId(Guid Value) : StronglyTypedId<Guid>(Value);
public sealed record InsuranceCalculationSnapshotItemId(Guid Value) : StronglyTypedId<Guid>(Value);
public sealed record InsuranceAuditLogId(Guid Value) : StronglyTypedId<Guid>(Value);
