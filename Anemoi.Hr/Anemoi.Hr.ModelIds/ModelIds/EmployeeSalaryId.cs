using Anemoi.BuildingBlock.Domain;

namespace Anemoi.Hr.ModelIds.ModelIds;

public sealed record EmployeeSalaryId(Guid Value) : StronglyTypedId<Guid>(Value);
