namespace Anemoi.Contract.Hr.Events;

public sealed record TransferApprovedIntegrationEvent(
    Guid TransferId,
    Guid EmployeeId,
    string FromDepartment,
    string ToDepartment,
    string FromPosition,
    string ToPosition);
