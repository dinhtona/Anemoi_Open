using Anemoi.BuildingBlock.Domain;

public sealed record ShiftTemplateId(Guid Value) : StronglyTypedId<Guid>(Value);
