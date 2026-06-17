using Anemoi.BuildingBlock.Domain;

namespace Anemoi.Hr.ModelIds.ModelIds;

public sealed record RecruitmentRequestId(Guid Value) : StronglyTypedId<Guid>(Value);
