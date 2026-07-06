using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Domain.Organization;

public sealed record ReportingRelationship(
    EmployeeId EmployeeId,
    string EmployeeName,
    EmployeeId? ManagerEmployeeId,
    string? ManagerName,
    string Level);
