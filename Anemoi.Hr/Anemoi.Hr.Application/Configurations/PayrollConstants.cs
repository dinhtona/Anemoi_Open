namespace Anemoi.Hr.Application.Configurations;

public static class PayrollConstants
{
    public const string BaseSalaryItemCode = "BASE_SALARY";
    public const string BaseSalaryItemName = "Base Salary";

    public const string AllowanceItemCodeFallback = "ALLOWANCE";
    public const string AllowanceItemNameFallback = "Allowance";

    public const string OvertimeItemCode = "OVERTIME";
    public const string OvertimeItemName = "Overtime Pay";

    public const string TaxDeductionItemCode = "TAX_DEDUCTION";
    public const string TaxDeductionItemName = "Income Tax";

    public const string InsuranceDeductionItemCode = "INSURANCE_DEDUCTION";
    public const string InsuranceDeductionItemName = "Insurance";

    public const decimal StandardWorkingHoursPerDay = 8m;
    public const decimal OvertimeRateMultiplier = 1.5m;

    public const string SystemActor = "system";

    public const string PayslipStatusNotGenerated = "NotGenerated";

    public const string FallbackDepartmentName = "No Department";
    public const string FallbackPositionName = "No Position";

    public const string Unknown = "Unknown";

    public const string CompensationTimelineEventSalary = "Salary";
    public const string CompensationTimelineEventAllowance = "Allowance";
    public const string CompensationTimelineActionChanged = "Changed";
    public const string CompensationTimelineActionAssigned = "Assigned";
    public const string CompensationTimelineActionTerminated = "Terminated";
}
