namespace Anemoi.Contract.Hr.Events;

public sealed record LeaveBalanceChangedIntegrationEvent(
    string EmployeeId,
    string LeavePolicyId,
    int Year,
    decimal RemainingDays,
    string TransactionTypeCode);
