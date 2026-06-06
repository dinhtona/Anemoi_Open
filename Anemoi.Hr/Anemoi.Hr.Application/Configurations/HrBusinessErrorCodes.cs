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
    public const string LeaveRequestOverlapping = "HR_LEAVE_REQUEST_OVERLAPPING";
    public const string LeaveRequestMultiYearNotSupported = "HR_LEAVE_REQUEST_MULTI_YEAR_NOT_SUPPORTED";
    public const string DepartmentTransferOverlapping = "HR_DEPARTMENT_TRANSFER_OVERLAPPING";
    public const string DepartmentTransferSameDepartment = "HR_DEPARTMENT_TRANSFER_SAME_DEPARTMENT";
    public const string DepartmentTransferDateBeforeJoinDate = "HR_DEPARTMENT_TRANSFER_DATE_BEFORE_JOIN_DATE";
    public const string PromotionNoChange = "HR_PROMOTION_NO_CHANGE";
    public const string PositionChangeSamePosition = "HR_POSITION_CHANGE_SAME_POSITION";
    public const string GradeChangeSameGrade = "HR_GRADE_CHANGE_SAME_GRADE";
    public const string PositionChangeOverlapping = "HR_POSITION_CHANGE_OVERLAPPING";
    public const string GradeChangeOverlapping = "HR_GRADE_CHANGE_OVERLAPPING";
    public const string GradeNotFound = "HR_GRADE_NOT_FOUND";
    public const string PromotionDateBeforeJoinDate = "HR_PROMOTION_DATE_BEFORE_JOIN_DATE";
    public const string PromotionEffectiveDateInFuture = "HR_PROMOTION_EFFECTIVE_DATE_IN_FUTURE";
    public const string PromotionConcurrencyConflict = "HR_PROMOTION_CONCURRENCY_CONFLICT";
    public const string ContractOverlapping = "HR_CONTRACT_OVERLAPPING";
    public const string ContractNotFound = "HR_CONTRACT_NOT_FOUND";
    public const string ContractNumberDuplicated = "HR_CONTRACT_NUMBER_DUPLICATED";
    public const string ContractAlreadyTerminated = "HR_CONTRACT_ALREADY_TERMINATED";
    public const string ContractAlreadyExpired = "HR_CONTRACT_ALREADY_EXPIRED";
    public const string ContractInvalidDateRange = "HR_CONTRACT_INVALID_DATE_RANGE";
    public const string ContractConcurrencyConflict = "HR_CONTRACT_CONCURRENCY_CONFLICT";
    public const string ContractNotDraft = "HR_CONTRACT_NOT_DRAFT";
    
    public const string PayrollPeriodNotFound = "HR_PAYROLL_PERIOD_NOT_FOUND";
    public const string PayrollPeriodCodeAlreadyExists = "HR_PAYROLL_PERIOD_CODE_ALREADY_EXISTS";
    public const string PayrollPeriodLocked = "HR_PAYROLL_PERIOD_LOCKED";
    public const string PayrollRunAlreadyExists = "HR_PAYROLL_RUN_ALREADY_EXISTS";
    public const string PayrollRunNotFound = "HR_PAYROLL_RUN_NOT_FOUND";
    public const string EmployeeSalaryNotFound = "HR_EMPLOYEE_SALARY_NOT_FOUND";
    public const string PayrollInvalidWorkingDays = "HR_PAYROLL_INVALID_WORKING_DAYS";
    public const string PayrollRunInvalidStatus = "HR_PAYROLL_RUN_INVALID_STATUS";
    public const string PayrollRunRejectionReasonRequired = "HR_PAYROLL_RUN_REJECTION_REASON_REQUIRED";
    public const string PayrollRunLocked = "HR_PAYROLL_RUN_LOCKED";

    public const string AttendancePeriodNotFound = "HR_ATTENDANCE_PERIOD_NOT_FOUND";
    public const string AttendanceRecordNotFound = "HR_ATTENDANCE_RECORD_NOT_FOUND";
    public const string AttendancePeriodDuplicated = "HR_ATTENDANCE_PERIOD_DUPLICATED";
    public const string AttendancePeriodLocked = "HR_ATTENDANCE_PERIOD_LOCKED";
    public const string AttendanceRecordAlreadyExists = "HR_ATTENDANCE_RECORD_ALREADY_EXISTS";
    public const string AttendanceInvalidTimeRange = "HR_ATTENDANCE_INVALID_TIME_RANGE";
    public const string AttendanceConcurrencyConflict = "HR_ATTENDANCE_CONCURRENCY_CONFLICT";

    public const string PayrollStandardWorkingDaysInvalid = "HR_PAYROLL_STANDARD_WORKING_DAYS_INVALID";
    public const string PayrollAttendancePeriodNotLinked = "HR_PAYROLL_ATTENDANCE_PERIOD_NOT_LINKED";
    public const string AttendanceSummaryNotFound = "HR_ATTENDANCE_SUMMARY_NOT_FOUND";
    public const string AttendancePeriodNotLocked = "HR_ATTENDANCE_PERIOD_NOT_LOCKED";
}

