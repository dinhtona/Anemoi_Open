using System;

namespace Anemoi.Hr.Application.Responses;

public sealed class CompensationTimelineItemResponse
{
    public DateOnly Date { get; set; }
    public string EventType { get; set; } // "Salary" or "Allowance"
    public string Action { get; set; } // "Changed", "Assigned", "Terminated"
    public string Description { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; }
    public string CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
}
