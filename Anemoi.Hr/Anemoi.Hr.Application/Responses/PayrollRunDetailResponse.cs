using System;
using System.Collections.Generic;

namespace Anemoi.Hr.Application.Responses;

public sealed class PayrollRunDetailResponse
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

    // Audit
    public DateTime CalculatedAt { get; set; }
    public string CalculatedBy { get; set; }

    // Detail Items
    public List<PayrollItemResponse> PayrollItems { get; set; } = [];
}
