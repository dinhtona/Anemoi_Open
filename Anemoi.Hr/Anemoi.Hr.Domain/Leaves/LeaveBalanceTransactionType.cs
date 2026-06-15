namespace Anemoi.Hr.Domain.Leaves;

public static class LeaveBalanceTransactionType
{
    public const string Refund = "Refund";
    public const string PendingRelease = "PendingRelease";
    public const string PendingReserve = "PendingReserve";
    public const string Used = "Used";

    public const string SourceTypeLeaveRequest = "LeaveRequest";
}
