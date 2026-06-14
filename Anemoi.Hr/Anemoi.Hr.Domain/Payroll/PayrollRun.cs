using Anemoi.BuildingBlock.Domain;
using Anemoi.Hr.Domain.Employees;
using Anemoi.Hr.ModelIds.ModelIds;
using System;
using System.Collections.Generic;

namespace Anemoi.Hr.Domain.Payroll;

public sealed class PayrollRun : ValueObject
{
    public PayrollRunId Id { get; set; }
    public PayrollPeriodId PayrollPeriodId { get; set; }
    public EmployeeId EmployeeId { get; set; }

    // Employee snapshot
    public string EmployeeCode { get; set; }
    public string EmployeeName { get; set; }

    // Department snapshot
    public DepartmentId? DepartmentIdSnapshot { get; set; }
    public string DepartmentNameSnapshot { get; set; }

    // Compensation snapshot
    public decimal BaseSalary { get; set; }
    public string CurrencyCode { get; set; }
    public string PayScheduleType { get; set; }

    // Working days and calculation details
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
    public PayrollRunStatus Status { get; private set; } = PayrollRunStatus.Calculated;
    public DateTime CalculatedAt { get; set; }
    public string CalculatedBy { get; set; }

    public string SubmittedBy { get; private set; }
    public DateTime? SubmittedAt { get; private set; }

    public string ApprovedBy { get; private set; }
    public DateTime? ApprovedAt { get; private set; }

    public string RejectedBy { get; private set; }
    public DateTime? RejectedAt { get; private set; }
    public string RejectionReason { get; private set; }

    public string FinalizedBy { get; private set; }
    public DateTime? FinalizedAt { get; private set; }

    public string CancelledBy { get; private set; }
    public DateTime? CancelledAt { get; private set; }
    public string CancellationReason { get; private set; }

    // Navigation
    public PayrollPeriod PayrollPeriod { get; set; }
    public Employee Employee { get; set; }
    public List<PayrollItem> PayrollItems { get; set; } = [];

    // Domain methods for lifecycle transitions
    public bool SubmitForApproval(string actor, DateTime now)
    {
        if (Status != PayrollRunStatus.Calculated)
            return false;

        Status = PayrollRunStatus.SubmittedForApproval;
        SubmittedBy = actor;
        SubmittedAt = now;
        return true;
    }

    public bool Approve(string actor, DateTime now)
    {
        if (Status != PayrollRunStatus.SubmittedForApproval)
            return false;

        Status = PayrollRunStatus.Approved;
        ApprovedBy = actor;
        ApprovedAt = now;
        return true;
    }

    public bool Reject(string actor, DateTime now, string reason)
    {
        if (Status != PayrollRunStatus.SubmittedForApproval)
            return false;

        if (string.IsNullOrWhiteSpace(reason))
            return false;

        Status = PayrollRunStatus.Rejected;
        RejectedBy = actor;
        RejectedAt = now;
        RejectionReason = reason;
        return true;
    }

    public bool FinalizeRun(string actor, DateTime now)
    {
        if (Status != PayrollRunStatus.Approved)
            return false;

        Status = PayrollRunStatus.Finalized;
        FinalizedBy = actor;
        FinalizedAt = now;
        return true;
    }

    public bool Cancel(string actor, DateTime now, string reason)
    {
        if (Status == PayrollRunStatus.Finalized || Status == PayrollRunStatus.Cancelled)
            return false;

        Status = PayrollRunStatus.Cancelled;
        CancelledBy = actor;
        CancelledAt = now;
        CancellationReason = reason;
        return true;
    }

    public void MarkRecalculated(string actor, DateTime now)
    {
        // Transition from Rejected back to Calculated and clear metadata
        if (Status == PayrollRunStatus.Rejected)
        {
            RejectedBy = null;
            RejectedAt = null;
            RejectionReason = null;
        }
        
        Status = PayrollRunStatus.Calculated;
        CalculatedBy = actor;
        CalculatedAt = now;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Id;
    }
}
