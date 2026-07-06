using Anemoi.BuildingBlock.Domain;

namespace Anemoi.Hr.ModelIds.ModelIds;

public sealed record EmployeeDocumentId(Guid Value) : StronglyTypedId<Guid>(Value);
