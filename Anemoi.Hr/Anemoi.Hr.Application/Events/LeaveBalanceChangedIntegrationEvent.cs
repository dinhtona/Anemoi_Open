namespace Anemoi.Hr.Application.Events;

public sealed record LeaveBalanceChangedIntegrationEvent(
    string EmployeeId,
    string LeavePolicyId,
    int Year,
    decimal RemainingDays,
    string TransactionTypeCode);
