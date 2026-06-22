using Anemoi.BuildingBlock.Domain;

namespace Anemoi.Hr.ModelIds.ModelIds;

public sealed record EmployeeOrganizationHistoryId(Guid Value) : StronglyTypedId<Guid>(Value);
