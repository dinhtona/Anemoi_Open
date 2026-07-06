using Anemoi.BuildingBlock.Domain;

namespace Anemoi.Hr.ModelIds.ModelIds;

public sealed record EmployeeManagerHistoryId(Guid Value) : StronglyTypedId<Guid>(Value);
