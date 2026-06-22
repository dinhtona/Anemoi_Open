using Anemoi.BuildingBlock.Domain;

namespace Anemoi.Hr.ModelIds.ModelIds;

public sealed record EmployeeHistoryId(Guid Value) : StronglyTypedId<Guid>(Value);
