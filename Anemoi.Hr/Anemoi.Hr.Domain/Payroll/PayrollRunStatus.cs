namespace Anemoi.Hr.Domain.Payroll;

public enum PayrollRunStatus
{
    Calculated,
    SubmittedForApproval,
    Approved,
    Rejected,
    Finalized,
    Cancelled
}
