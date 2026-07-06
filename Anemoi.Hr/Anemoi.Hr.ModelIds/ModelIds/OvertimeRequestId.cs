using Anemoi.BuildingBlock.Domain;

public sealed record OvertimeRequestId(Guid Value) : StronglyTypedId<Guid>(Value);
