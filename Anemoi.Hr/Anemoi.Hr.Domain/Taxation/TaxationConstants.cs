namespace Anemoi.Hr.Domain.Taxation;

public static class TaxRuleSetStatusCode
{
    public const string Draft = "Draft";
    public const string Active = "Active";
    public const string Inactive = "Inactive";
}

public static class TaxDeductionTypeCode
{
    public const string PersonalDeduction = "PersonalDeduction";
    public const string DependentDeduction = "DependentDeduction";
}

public static class TaxDeductionInputKey
{
    public const string Dependents = "Dependents";
    public const string DependentCount = "DependentCount";
}

public static class HrSourceModuleCode
{
    public const string Payroll = "Payroll";
}
