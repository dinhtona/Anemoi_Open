using Anemoi.BuildingBlock.Domain;

namespace Anemoi.Hr.ModelIds.ModelIds;

public sealed record EmployeePositionHistoryId(Guid Value) : StronglyTypedId<Guid>(Value);
