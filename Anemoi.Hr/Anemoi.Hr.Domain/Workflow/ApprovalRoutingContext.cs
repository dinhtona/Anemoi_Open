using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Domain.Workflow;

public sealed record ApprovalRoutingContext(
    EmployeeId RequesterEmployeeId,
    DepartmentId? DepartmentId,
    PositionId? PositionId,
    string EntityType,
    string? EntityId);
