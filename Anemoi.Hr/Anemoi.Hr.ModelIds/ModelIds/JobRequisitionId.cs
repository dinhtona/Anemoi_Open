using Anemoi.BuildingBlock.Domain;

namespace Anemoi.Hr.ModelIds.ModelIds;

public sealed record JobRequisitionId(Guid Value) : StronglyTypedId<Guid>(Value);
