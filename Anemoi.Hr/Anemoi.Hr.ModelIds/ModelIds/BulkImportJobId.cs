using Anemoi.BuildingBlock.Domain;

namespace Anemoi.Hr.ModelIds.ModelIds;

public sealed record BulkImportJobId(Guid Value) : StronglyTypedId<Guid>(Value);
