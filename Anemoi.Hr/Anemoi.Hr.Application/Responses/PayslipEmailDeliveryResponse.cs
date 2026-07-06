using System;

namespace Anemoi.Hr.Application.Responses;

public sealed class PayslipEmailDeliveryResponse
{
    public Guid Id { get; set; }
    public Guid PayslipId { get; set; }
    public Guid PayslipDocumentId { get; set; }
    public string ToEmail { get; set; } = default!;
    public string Subject { get; set; } = default!;
    public string Status { get; set; } = default!;
    public string? ErrorMessage { get; set; }
    public string SentBy { get; set; } = default!;
    public DateTime? SentAt { get; set; }
    public DateTime? FailedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
