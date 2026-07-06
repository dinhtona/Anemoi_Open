using Anemoi.BuildingBlock.Domain;

namespace Anemoi.Hr.ModelIds.ModelIds;

public sealed record WorkflowRoleAssignmentId(Guid Value) : StronglyTypedId<Guid>(Value);
