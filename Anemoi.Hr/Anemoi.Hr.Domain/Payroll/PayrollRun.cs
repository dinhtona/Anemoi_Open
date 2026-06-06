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

    // Compensation snapshot
    public decimal BaseSalary { get; set; }
    public string CurrencyCode { get; set; }
    public string PayScheduleType { get; set; }
    public decimal TotalAllowanceAmount { get; set; }
    public decimal GrossAmount { get; set; }

    // Audit
    public DateTime CalculatedAt { get; set; }
    public string CalculatedBy { get; set; }

    // Navigation
    public PayrollPeriod PayrollPeriod { get; set; }
    public Employee Employee { get; set; }
    public List<PayrollItem> PayrollItems { get; set; } = [];

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Id;
    }
}
