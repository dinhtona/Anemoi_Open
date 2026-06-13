using Anemoi.BuildingBlock.Domain;
using Anemoi.Hr.ModelIds.ModelIds;
using System;

namespace Anemoi.Hr.Domain.Payroll;

public sealed class PayslipEmailDelivery : Entity<PayslipEmailDeliveryId>
{
    public PayslipId PayslipId { get; private set; }
    public PayslipDocumentId PayslipDocumentId { get; private set; }
    public string ToEmail { get; private set; } = default!;
    public string Subject { get; private set; } = default!;
    public PayslipEmailDeliveryStatus Status { get; private set; }
    public string? ErrorMessage { get; private set; }
    public string SentBy { get; private set; } = default!;
    public DateTime? SentAt { get; private set; }
    public DateTime? FailedAt { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    // Navigation
    public Payslip Payslip { get; private set; } = default!;
    public PayslipDocument PayslipDocument { get; private set; } = default!;

    private PayslipEmailDelivery() { }

    public static PayslipEmailDelivery Create(
        PayslipId payslipId,
        PayslipDocumentId payslipDocumentId,
        string toEmail,
        string subject,
        string sentBy)
    {
        return new PayslipEmailDelivery
        {
            Id = new PayslipEmailDeliveryId(Guid.NewGuid()),
            PayslipId = payslipId,
            PayslipDocumentId = payslipDocumentId,
            ToEmail = toEmail,
            Subject = subject,
            Status = PayslipEmailDeliveryStatus.Pending,
            SentBy = sentBy,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void MarkSent(DateTime sentAt)
    {
        Status = PayslipEmailDeliveryStatus.Sent;
        SentAt = sentAt;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkFailed(string errorMessage, DateTime failedAt)
    {
        Status = PayslipEmailDeliveryStatus.Failed;
        ErrorMessage = errorMessage;
        FailedAt = failedAt;
        UpdatedAt = DateTime.UtcNow;
    }
}
