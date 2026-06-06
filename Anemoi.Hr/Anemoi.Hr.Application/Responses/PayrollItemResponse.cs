namespace Anemoi.Hr.Application.Responses;

public sealed class PayrollItemResponse
{
    public string Id { get; set; }
    public string PayrollRunId { get; set; }
    public string ItemCode { get; set; }
    public string ItemName { get; set; }
    public string ItemTypeCode { get; set; }
    public decimal Amount { get; set; }
    public string CurrencyCode { get; set; }
}
