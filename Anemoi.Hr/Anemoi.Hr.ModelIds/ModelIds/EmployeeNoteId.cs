using Anemoi.BuildingBlock.Domain;

namespace Anemoi.Hr.ModelIds.ModelIds;

public sealed record EmployeeNoteId(Guid Value) : StronglyTypedId<Guid>(Value);
