using System;

namespace Anemoi.Hr.Application.Responses;

public sealed class PayrollRunResponse
{
    public string Id { get; set; }
    public string PayrollPeriodId { get; set; }
    public string EmployeeId { get; set; }

    // Employee snapshot
    public string EmployeeCode { get; set; }
    public string EmployeeName { get; set; }

    // Compensation snapshot
    public decimal BaseSalary { get; set; }
    public string CurrencyCode { get; set; }
    public string PayScheduleType { get; set; }

    // Working days and details
    public decimal StandardWorkingDays { get; set; }
    public decimal PaidWorkingDays { get; set; }
    public decimal UnpaidLeaveDays { get; set; }
    public decimal DailyRate { get; set; }
    public decimal BasePayAmount { get; set; }
    public decimal TotalAllowanceAmount { get; set; }
    public decimal GrossAmount { get; set; }
    public decimal TotalDeductionAmount { get; set; }
    public decimal NetAmount { get; set; }

    // Status & Audit
    public string Status { get; set; }
    public DateTime CalculatedAt { get; set; }
    public string CalculatedBy { get; set; }

    public string SubmittedBy { get; set; }
    public DateTime? SubmittedAt { get; set; }

    public string ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }

    public string RejectedBy { get; set; }
    public DateTime? RejectedAt { get; set; }
    public string RejectionReason { get; set; }

    public string FinalizedBy { get; set; }
    public DateTime? FinalizedAt { get; set; }

    public string CancelledBy { get; set; }
    public DateTime? CancelledAt { get; set; }
    public string CancellationReason { get; set; }
}
