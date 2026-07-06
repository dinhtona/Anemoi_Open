using Anemoi.BuildingBlock.Domain;

namespace Anemoi.Hr.ModelIds.ModelIds;

public sealed record EmployeeGradeHistoryId(Guid Value) : StronglyTypedId<Guid>(Value);
