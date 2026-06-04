namespace Anemoi.Hr.Application.Responses;

public sealed class LeaveTransactionResponse
{
    public string Id { get; set; }
    public string EmployeeId { get; set; }
    public string LeavePolicyId { get; set; }
    public string LeaveBalanceId { get; set; }
    public string LeaveRequestId { get; set; }
    public string TransactionTypeCode { get; set; }
    public decimal Days { get; set; }
    public decimal BalanceAfterDays { get; set; }
    public string Reason { get; set; }
    public DateTime CreatedAt { get; set; }
}
