using Anemoi.BuildingBlock.Domain;

namespace Anemoi.Hr.ModelIds.ModelIds;

public sealed record EmployeeDepartmentHistoryId(Guid Value) : StronglyTypedId<Guid>(Value);
