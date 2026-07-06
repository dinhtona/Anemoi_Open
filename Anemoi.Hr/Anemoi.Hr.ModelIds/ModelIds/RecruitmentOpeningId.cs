using Anemoi.BuildingBlock.Domain;

namespace Anemoi.Hr.ModelIds.ModelIds;

public sealed record RecruitmentOpeningId(Guid Value) : StronglyTypedId<Guid>(Value);
