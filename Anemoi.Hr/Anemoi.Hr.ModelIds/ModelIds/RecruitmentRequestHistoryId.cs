using Anemoi.BuildingBlock.Domain;

namespace Anemoi.Hr.ModelIds.ModelIds;

public sealed record RecruitmentRequestHistoryId(Guid Value) : StronglyTypedId<Guid>(Value);
