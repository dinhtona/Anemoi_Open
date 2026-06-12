using Anemoi.BuildingBlock.Domain;

public sealed record EmployeeShiftAssignmentId(Guid Value) : StronglyTypedId<Guid>(Value);
