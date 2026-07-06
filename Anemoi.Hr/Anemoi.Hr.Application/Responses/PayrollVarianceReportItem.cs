#nullable enable

using System;

namespace Anemoi.Hr.Application.Responses;

public sealed record PayrollVarianceReportItem
{
    public Guid? CurrentPayrollRunId { get; init; }
    public Guid? PreviousPayrollRunId { get; init; }
    public Guid EmployeeId { get; init; }
    public string EmployeeCode { get; init; } = default!;
    public string EmployeeName { get; init; } = default!;
    public decimal CurrentGrossIncome { get; init; }
    public decimal PreviousGrossIncome { get; init; }
    public decimal GrossIncomeDifference { get; init; }
    public decimal CurrentNetPay { get; init; }
    public decimal PreviousNetPay { get; init; }
    public decimal NetPayDifference { get; init; }
    public decimal CurrentTax { get; init; }
    public decimal PreviousTax { get; init; }
    public decimal TaxDifference { get; init; }
    public decimal CurrentInsurance { get; init; }
    public decimal PreviousInsurance { get; init; }
    public decimal InsuranceDifference { get; init; }
}
