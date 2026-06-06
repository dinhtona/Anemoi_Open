using Anemoi.BuildingBlock.Domain;

namespace Anemoi.Hr.ModelIds.ModelIds;

public sealed record SalaryValidationBypassLogId(Guid Value) : StronglyTypedId<Guid>(Value);
