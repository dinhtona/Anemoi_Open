namespace Anemoi.Hr.Application.Configurations;

public static class HrBusinessErrorCodes
{
    public const string LeavePolicyNotFound = "HR_LEAVE_POLICY_NOT_FOUND";
    public const string LeavePolicyCodeAlreadyExists = "HR_LEAVE_POLICY_CODE_ALREADY_EXISTS";
    public const string LeaveBalanceNotFound = "HR_LEAVE_BALANCE_NOT_FOUND";
    public const string LeaveBalanceNotEnough = "HR_LEAVE_BALANCE_NOT_ENOUGH";
    public const string LeaveBalanceConcurrencyConflict = "HR_LEAVE_BALANCE_CONCURRENCY_CONFLICT";
    public const string LeaveRequestNotFound = "HR_LEAVE_REQUEST_NOT_FOUND";
    public const string LeaveRequestAlreadyApproved = "HR_LEAVE_REQUEST_ALREADY_APPROVED";
    public const string LeaveRequestAlreadyRejected = "HR_LEAVE_REQUEST_ALREADY_REJECTED";
    public const string LeaveRequestAlreadyCancelled = "HR_LEAVE_REQUEST_ALREADY_CANCELLED";
    public const string LeaveRequestInvalidStatus = "HR_LEAVE_REQUEST_INVALID_STATUS";
    public const string LeaveRequestInvalidDateRange = "HR_LEAVE_REQUEST_INVALID_DATE_RANGE";
    public const string LeaveAdjustmentReasonRequired = "HR_LEAVE_ADJUSTMENT_REASON_REQUIRED";
    public const string PermissionSensitiveConfirmationRequired = "HR_PERMISSION_SENSITIVE_CONFIRMATION_REQUIRED";
    public const string EmployeeNotFound = "HR_EMPLOYEE_NOT_FOUND";
    public const string DepartmentNotFound = "HR_DEPARTMENT_NOT_FOUND";
    public const string PositionNotFound = "HR_POSITION_NOT_FOUND";
}
