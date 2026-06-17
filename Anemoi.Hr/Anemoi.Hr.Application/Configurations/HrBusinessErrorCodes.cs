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
    public const string AttendancePeriodAlreadyLocked = "HR_ATTENDANCE_PERIOD_ALREADY_LOCKED";
    public const string AttendanceRecordAlreadyExists = "HR_ATTENDANCE_RECORD_ALREADY_EXISTS";
    public const string AttendanceInvalidTimeRange = "HR_ATTENDANCE_INVALID_TIME_RANGE";
    public const string AttendanceConcurrencyConflict = "HR_ATTENDANCE_CONCURRENCY_CONFLICT";

    public const string PayrollStandardWorkingDaysInvalid = "HR_PAYROLL_STANDARD_WORKING_DAYS_INVALID";
    public const string PayrollAttendancePeriodNotLinked = "HR_PAYROLL_ATTENDANCE_PERIOD_NOT_LINKED";
    public const string AttendanceSummaryNotFound = "HR_ATTENDANCE_SUMMARY_NOT_FOUND";
    public const string AttendancePeriodNotLocked = "HR_ATTENDANCE_PERIOD_NOT_LOCKED";

    public const string PayrollPeriodAttendancePeriodRequired = "HR_PAYROLL_PERIOD_ATTENDANCE_PERIOD_REQUIRED";
    public const string AttendanceRecordsNotFoundForPeriod = "HR_ATTENDANCE_RECORDS_NOT_FOUND_FOR_PERIOD";
    public const string AttendanceSummaryAlreadyExists = "HR_ATTENDANCE_SUMMARY_ALREADY_EXISTS";
    public const string AttendanceSummaryGenerationFailed = "HR_ATTENDANCE_SUMMARY_GENERATION_FAILED";

    public const string PayrollRunNotFinalized = "HR_PAYROLL_RUN_NOT_FINALIZED";
    public const string PayslipInvalidStatus = "HR_PAYSLIP_INVALID_STATUS";
    public const string ReportExportFileGenerationFailed = "HR_REPORT_EXPORT_FILE_GENERATION_FAILED";
    public const string ReportExportPermissionDenied = "HR_REPORT_EXPORT_PERMISSION_DENIED";
    public const string ReportExportAuditLogSaveFailed = "HR_REPORT_EXPORT_AUDIT_LOG_SAVE_FAILED";
    public const string ReportExportLimitExceeded = "HR_REPORT_EXPORT_LIMIT_EXCEEDED";
    public const string ReportPageInvalid = "HR_REPORT_PAGE_INVALID";
    public const string ReportPageSizeInvalid = "HR_REPORT_PAGE_SIZE_INVALID";
    public const string ReportDateRangeInvalid = "HR_REPORT_DATE_RANGE_INVALID";
    public const string ReportSortDirectionInvalid = "HR_REPORT_SORT_DIRECTION_INVALID";
    public const string ReportStatusInvalid = "HR_REPORT_STATUS_INVALID";

    // Overtime error codes
    public const string OvertimeRequestNotFound = "HR_OVERTIME_REQUEST_NOT_FOUND";
    public const string OvertimeDateInvalid = "HR_OVERTIME_DATE_INVALID";
    public const string OvertimeTimeInvalid = "HR_OVERTIME_TIME_INVALID";
    public const string OvertimeDurationExceedsLimit = "HR_OVERTIME_DURATION_EXCEEDS_LIMIT";
    public const string OvertimeReasonInvalid = "HR_OVERTIME_REASON_INVALID";
    public const string OverlappingOvertimeRequestsNotAllowed = "HR_OVERLAPPING_OVERTIME_REQUESTS_NOT_ALLOWED";
    public const string OvertimeApproverRequired = "HR_OVERTIME_APPROVER_REQUIRED";
    public const string OvertimeRequestConcurrencyConflict = "HR_OVERTIME_REQUEST_CONCURRENCY_CONFLICT";

    // Shift Management error codes
    public const string AssignedByRequired = "HR_ASSIGNED_BY_REQUIRED";
    public const string CancelledByRequired = "HR_CANCELLED_BY_REQUIRED";
    public const string CancellationReasonRequired = "HR_CANCELLATION_REASON_REQUIRED";
    public const string CancellationReasonMaxLength = "HR_CANCELLATION_REASON_MAX_LENGTH";
    public const string ShiftTemplateNotFound = "HR_SHIFT_TEMPLATE_NOT_FOUND";
    public const string ShiftTemplateCodeAlreadyExists = "HR_SHIFT_TEMPLATE_CODE_ALREADY_EXISTS";
    public const string ShiftTemplateInvalidTimeRange = "HR_SHIFT_TEMPLATE_INVALID_TIME_RANGE";
    public const string ShiftTemplateInvalidBreakMinutes = "HR_SHIFT_TEMPLATE_INVALID_BREAK_MINUTES";
    public const string ShiftTemplateInactive = "HR_SHIFT_TEMPLATE_INACTIVE";
    public const string ShiftAssignmentNotFound = "HR_SHIFT_ASSIGNMENT_NOT_FOUND";
    public const string ShiftAssignmentDuplicate = "HR_SHIFT_ASSIGNMENT_DUPLICATE";
    public const string ShiftAssignmentOverlap = "HR_SHIFT_ASSIGNMENT_OVERLAP";
    public const string ShiftAssignmentInvalidStatus = "HR_SHIFT_ASSIGNMENT_INVALID_STATUS";
    public const string ShiftTemplateCodeRequired = "HR_SHIFT_TEMPLATE_CODE_REQUIRED";
    public const string ShiftTemplateNameRequired = "HR_SHIFT_TEMPLATE_NAME_REQUIRED";
    public const string ShiftAssignmentConcurrencyConflict = "HR_SHIFT_ASSIGNMENT_CONCURRENCY_CONFLICT";
    // Calendar Management error codes
    public const string PublicHolidayNotFound = "HR_PUBLIC_HOLIDAY_NOT_FOUND";
    public const string PublicHolidayDuplicate = "HR_PUBLIC_HOLIDAY_DUPLICATE";
    public const string PublicHolidayDateRequired = "HR_PUBLIC_HOLIDAY_DATE_REQUIRED";
    public const string PublicHolidayNameRequired = "HR_PUBLIC_HOLIDAY_NAME_REQUIRED";
    public const string PublicHolidayCountryCodeRequired = "HR_PUBLIC_HOLIDAY_COUNTRY_CODE_REQUIRED";
    public const string PublicHolidayConcurrencyConflict = "HR_PUBLIC_HOLIDAY_CONCURRENCY_CONFLICT";

    public const string CompanyHolidayNotFound = "HR_COMPANY_HOLIDAY_NOT_FOUND";
    public const string CompanyHolidayDuplicate = "HR_COMPANY_HOLIDAY_DUPLICATE";
    public const string CompanyHolidayDateRequired = "HR_COMPANY_HOLIDAY_DATE_REQUIRED";
    public const string CompanyHolidayNameRequired = "HR_COMPANY_HOLIDAY_NAME_REQUIRED";
    public const string CompanyHolidayConcurrencyConflict = "HR_COMPANY_HOLIDAY_CONCURRENCY_CONFLICT";

    public const string WorkingCalendarRuleNotFound = "HR_WORKING_CALENDAR_RULE_NOT_FOUND";
    public const string WorkingCalendarRuleNameRequired = "HR_WORKING_CALENDAR_RULE_NAME_REQUIRED";
    public const string WorkingCalendarRuleEffectiveFromRequired = "HR_WORKING_CALENDAR_RULE_EFFECTIVE_FROM_REQUIRED";
    public const string WorkingCalendarRuleWorkingDaysRequired = "HR_WORKING_CALENDAR_RULE_WORKING_DAYS_REQUIRED";
    public const string WorkingCalendarRuleAlreadyActive = "HR_WORKING_CALENDAR_RULE_ALREADY_ACTIVE";
    public const string WorkingCalendarRuleAlreadyInactive = "HR_WORKING_CALENDAR_RULE_ALREADY_INACTIVE";
    public const string WorkingCalendarRuleConcurrencyConflict = "HR_WORKING_CALENDAR_RULE_CONCURRENCY_CONFLICT";
    public const string WorkingCalendarRuleOverlap = "HR_WORKING_CALENDAR_RULE_OVERLAP";

    public const string CalendarExceptionNotFound = "HR_CALENDAR_EXCEPTION_NOT_FOUND";
    public const string CalendarExceptionDuplicate = "HR_CALENDAR_EXCEPTION_DUPLICATE";
    public const string CalendarExceptionDateRequired = "HR_CALENDAR_EXCEPTION_DATE_REQUIRED";
    public const string CalendarExceptionReasonRequired = "HR_CALENDAR_EXCEPTION_REASON_REQUIRED";
    public const string CalendarExceptionTypeRequired = "HR_CALENDAR_EXCEPTION_TYPE_REQUIRED";
    public const string CalendarExceptionConcurrencyConflict = "HR_CALENDAR_EXCEPTION_CONCURRENCY_CONFLICT";

    public const string SaveChangesFailed = "HR_SAVE_CHANGES_FAILED";

    // Database persistence error codes
    public const string DbUniqueConstraint = "HR_DB_UNIQUE_CONSTRAINT";
    public const string DbForeignKeyViolation = "HR_DB_FOREIGN_KEY_VIOLATION";
    public const string DbCheckViolation = "HR_DB_CHECK_VIOLATION";

    public const string ShiftTemplateConcurrencyConflict = "HR_SHIFT_TEMPLATE_CONCURRENCY_CONFLICT";

    // Salary error codes
    public const string SalaryEffectiveDateInFuture = "HR_SALARY_EFFECTIVE_DATE_IN_FUTURE";
    public const string SalaryDateBeforeJoinDate = "HR_SALARY_DATE_BEFORE_JOIN_DATE";
    public const string SalaryTimelineNotSequential = "HR_SALARY_TIMELINE_NOT_SEQUENTIAL";
    public const string SalaryOutOfGradeRange = "HR_SALARY_OUT_OF_GRADE_RANGE";
    public const string SalaryConcurrencyConflict = "HR_SALARY_CONCURRENCY_CONFLICT";
    public const string SalaryGradeAlreadyExists = "HR_SALARY_GRADE_ALREADY_EXISTS";
    public const string SalaryGradeNotFound = "HR_SALARY_GRADE_NOT_FOUND";
    public const string SalaryRangeOverlapping = "HR_SALARY_RANGE_OVERLAPPING";

    // Allowance error codes
    public const string EmployeeAllowanceNotFound = "HR_EMPLOYEE_ALLOWANCE_NOT_FOUND";
    public const string AllowanceInvalidEffectiveRange = "HR_ALLOWANCE_INVALID_EFFECTIVE_RANGE";
    public const string AllowanceTimelineOverlap = "HR_ALLOWANCE_TIMELINE_OVERLAP";
    public const string AllowanceTimelineDuplicateDate = "HR_ALLOWANCE_TIMELINE_DUPLICATE_DATE";
    public const string AllowanceAlreadyUsedInPayroll = "HR_ALLOWANCE_ALREADY_USED_IN_PAYROLL";
    public const string AllowanceConcurrencyConflict = "HR_ALLOWANCE_CONCURRENCY_CONFLICT";
    public const string AllowanceTypeNotFound = "HR_ALLOWANCE_TYPE_NOT_FOUND";
    public const string AllowanceTypeCodeAlreadyExists = "HR_ALLOWANCE_TYPE_CODE_ALREADY_EXISTS";
    public const string AllowanceTypeConcurrencyConflict = "HR_ALLOWANCE_TYPE_CONCURRENCY_CONFLICT";
    public const string AllowanceTerminationDateInFuture = "HR_ALLOWANCE_TERMINATION_DATE_IN_FUTURE";
    public const string AllowanceTerminationBeforeEffectiveFrom = "HR_ALLOWANCE_TERMINATION_BEFORE_EFFECTIVE_FROM";
    public const string AllowanceEffectiveDateInFuture = "HR_ALLOWANCE_EFFECTIVE_DATE_IN_FUTURE";
    public const string AllowanceDateBeforeJoinDate = "HR_ALLOWANCE_DATE_BEFORE_JOIN_DATE";
    public const string AllowanceTimelineNotSequential = "HR_ALLOWANCE_TIMELINE_NOT_SEQUENTIAL";
    public const string PositionAllowanceAlreadyExists = "HR_POSITION_ALLOWANCE_ALREADY_EXISTS";

    // Payroll error codes
    public const string PayrollPeriodConcurrencyConflict = "HR_PAYROLL_PERIOD_CONCURRENCY_CONFLICT";
    public const string PayrollRunConcurrencyConflict = "HR_PAYROLL_RUN_CONCURRENCY_CONFLICT";

    // Tax error codes
    public const string TaxRuleSetNotFound = "HR_TAX_RULE_SET_NOT_FOUND";
    public const string TaxRuleSetNotDraft = "HR_TAX_RULE_SET_NOT_DRAFT";
    public const string TaxRuleSetInvalidDateRange = "HR_TAX_RULE_SET_INVALID_DATE_RANGE";
    public const string TaxRuleSetOverlappingPeriod = "HR_TAX_RULE_SET_OVERLAPPING_PERIOD";
    public const string TaxRuleSetOverlapWithActive = "HR_TAX_RULE_SET_OVERLAP_WITH_ACTIVE";
    public const string TaxRuleSetHasNoBrackets = "HR_TAX_RULE_SET_HAS_NO_BRACKETS";
    public const string TaxBracketNotFound = "HR_TAX_BRACKET_NOT_FOUND";
    public const string TaxBracketInvalidFromAmount = "HR_TAX_BRACKET_INVALID_FROM_AMOUNT";
    public const string TaxBracketInvalidToAmount = "HR_TAX_BRACKET_INVALID_TO_AMOUNT";
    public const string TaxBracketInvalidRate = "HR_TAX_BRACKET_INVALID_RATE";
    public const string TaxBracketOverlaps = "HR_TAX_BRACKET_OVERLAPS";
    public const string TaxDeductionRuleNotFound = "HR_TAX_DEDUCTION_RULE_NOT_FOUND";
    public const string TaxDeductionRuleInvalidAmount = "HR_TAX_DEDUCTION_RULE_INVALID_AMOUNT";
    public const string TaxDeductionRuleTypeAlreadyExists = "HR_TAX_DEDUCTION_RULE_TYPE_ALREADY_EXISTS";
    public const string TaxCalculationInvalidPeriod = "HR_TAX_CALCULATION_INVALID_PERIOD";
    public const string TaxCalculationNegativeIncome = "HR_TAX_CALCULATION_NEGATIVE_INCOME";
    public const string TaxCalculationSnapshotNotFound = "HR_TAX_CALCULATION_SNAPSHOT_NOT_FOUND";

    // Insurance error codes
    public const string InsuranceRuleSetNotFound = "HR_INSURANCE_RULE_SET_NOT_FOUND";
    public const string InsuranceRuleSetNotDraft = "HR_INSURANCE_RULE_SET_NOT_DRAFT";
    public const string InsuranceRuleSetNotActive = "HR_INSURANCE_RULE_SET_NOT_ACTIVE";
    public const string InsuranceRuleSetHasNoRules = "HR_INSURANCE_RULE_SET_HAS_NO_RULES";
    public const string InsuranceRuleSetOverlapWithActive = "HR_INSURANCE_RULE_SET_OVERLAP_WITH_ACTIVE";
    public const string InsuranceContributionRuleNotFound = "HR_INSURANCE_CONTRIBUTION_RULE_NOT_FOUND";
    public const string InsuranceCalculationInvalidPeriod = "HR_INSURANCE_CALCULATION_INVALID_PERIOD";
    public const string InsuranceCalculationNegativeSalary = "HR_INSURANCE_CALCULATION_NEGATIVE_SALARY";
    public const string InsuranceSnapshotNotFound = "HR_INSURANCE_SNAPSHOT_NOT_FOUND";

    // Report error codes
    public const string ReportVarianceCurrentCriteriaRequired = "HR_REPORT_VARIANCE_CURRENT_CRITERIA_REQUIRED";
    public const string ReportVariancePreviousCriteriaRequired = "HR_REPORT_VARIANCE_PREVIOUS_CRITERIA_REQUIRED";

    // Validation error codes
    public const string ValActorEmployeeIdRequired = "VAL_ACTOR_EMPLOYEE_ID_REQUIRED";
    public const string ValAllowanceTypeIdRequired = "VAL_ALLOWANCE_TYPE_ID_REQUIRED";
    public const string ValAmountMustBePositive = "VAL_AMOUNT_MUST_BE_POSITIVE";
    public const string ValApproverEmployeeIdRequired = "VAL_APPROVER_EMPLOYEE_ID_REQUIRED";
    public const string ValAttendancePeriodIdRequired = "VAL_ATTENDANCE_PERIOD_ID_REQUIRED";
    public const string ValAttendanceRecordIdRequired = "VAL_ATTENDANCE_RECORD_ID_REQUIRED";
    public const string ValAttendanceStatusUnsupported = "VAL_ATTENDANCE_STATUS_UNSUPPORTED";
    public const string ValBaseSalaryMustBePositive = "VAL_BASE_SALARY_MUST_BE_POSITIVE";
    public const string ValContractIdRequired = "VAL_CONTRACT_ID_REQUIRED";
    public const string ValCurrencyRequired = "VAL_CURRENCY_REQUIRED";
    public const string ValEffectiveFromRequired = "VAL_EFFECTIVE_FROM_REQUIRED";
    public const string ValEmployeeAllowanceIdRequired = "VAL_EMPLOYEE_ALLOWANCE_ID_REQUIRED";
    public const string ValEmployeeIdRequired = "VAL_EMPLOYEE_ID_REQUIRED";
    public const string ValEndDateBeforeStartDate = "VAL_END_DATE_BEFORE_START_DATE";
    public const string ValEndDateRequired = "VAL_END_DATE_REQUIRED";
    public const string ValLeaveBalanceIdRequired = "VAL_LEAVE_BALANCE_ID_REQUIRED";
    public const string ValLeavePolicyIdRequired = "VAL_LEAVE_POLICY_ID_REQUIRED";
    public const string ValLeaveRequestIdRequired = "VAL_LEAVE_REQUEST_ID_REQUIRED";
    public const string ValMaxSalaryMustBeGeMinSalary = "VAL_MAX_SALARY_MUST_BE_GE_MIN_SALARY";
    public const string ValMinSalaryMustBePositive = "VAL_MIN_SALARY_MUST_BE_POSITIVE";
    public const string ValNewDepartmentIdRequired = "VAL_NEW_DEPARTMENT_ID_REQUIRED";
    public const string ValOvertimeRequestIdRequired = "VAL_OVERTIME_REQUEST_ID_REQUIRED";
    public const string ValPayrollPeriodIdRequired = "VAL_PAYROLL_PERIOD_ID_REQUIRED";
    public const string ValPayrollRunIdRequired = "VAL_PAYROLL_RUN_ID_REQUIRED";
    public const string ValPeriodCodeRequired = "VAL_PERIOD_CODE_REQUIRED";
    public const string ValPeriodCodeTooLong = "VAL_PERIOD_CODE_TOO_LONG";
    public const string ValPositionIdRequired = "VAL_POSITION_ID_REQUIRED";
    public const string ValReasonInvalid = "VAL_REASON_INVALID";
    public const string ValSalaryGradeIdRequired = "VAL_SALARY_GRADE_ID_REQUIRED";
    public const string ValSalaryTypeInvalid = "VAL_SALARY_TYPE_INVALID";
    public const string ValStandardWorkingDaysMustBePos = "VAL_STANDARD_WORKING_DAYS_MUST_BE_POS";
    public const string ValStartDateRequired = "VAL_START_DATE_REQUIRED";
    public const string ValStatusRequired = "VAL_STATUS_REQUIRED";
    public const string ValWorkedDaysOutOfRange = "VAL_WORKED_DAYS_OUT_OF_RANGE";
    public const string ValWorkedHoursOutOfRange = "VAL_WORKED_HOURS_OUT_OF_RANGE";
    public const string ValWorkDateOutOfPeriod = "VAL_WORK_DATE_OUT_OF_PERIOD";
    public const string ValWorkDateRequired = "VAL_WORK_DATE_REQUIRED";
    public const string ValNameRequired = "VAL_NAME_REQUIRED";
    public const string ValTitleRequired = "VAL_TITLE_REQUIRED";

    // Validation error codes (HR-specific)
    public const string PayslipIdRequired = "VAL_PAYSLIP_ID_REQUIRED";
    public const string DocumentIdRequired = "VAL_DOCUMENT_ID_REQUIRED";
    public const string AtLeastOneInsuranceRateRequired = "HR_INSURANCE_AT_LEAST_ONE_RATE_REQUIRED";

    // Allowance validator error codes
    public const string AllowanceInvalidDateRange = "HR_ALLOWANCE_INVALID_DATE_RANGE";

    public const string PayslipNotFound = "HR_PAYSLIP_NOT_FOUND";
    public const string PayslipDocumentNotFound = "HR_PAYSLIP_DOCUMENT_NOT_FOUND";
    public const string PayslipDocumentAlreadyExists = "HR_PAYSLIP_DOCUMENT_ALREADY_EXISTS";
    public const string PayslipDocumentGenerationNotAllowed = "HR_PAYSLIP_DOCUMENT_GENERATION_NOT_ALLOWED";
    public const string PayslipDocumentGenerationFailed = "HR_PAYSLIP_DOCUMENT_GENERATION_FAILED";
    public const string PayslipDocumentStorageFailed = "HR_PAYSLIP_DOCUMENT_STORAGE_FAILED";
    public const string PayslipEmailNotFound = "HR_PAYSLIP_EMAIL_NOT_FOUND";
    public const string PayslipEmailRequired = "HR_PAYSLIP_EMAIL_REQUIRED";
    public const string PayslipEmailSendNotAllowed = "HR_PAYSLIP_EMAIL_SEND_NOT_ALLOWED";
    public const string PayslipEmailSendFailed = "HR_PAYSLIP_EMAIL_SEND_FAILED";
    public const string PayslipActiveDocumentRequired = "HR_PAYSLIP_ACTIVE_DOCUMENT_REQUIRED";

    // Recruitment error codes
    public const string RequisitionNotFound = "HR_REC_REQUISITION_NOT_FOUND";
    public const string RequisitionCodeAlreadyExists = "HR_REC_REQUISITION_CODE_ALREADY_EXISTS";
    public const string RequisitionInvalidStatus = "HR_REC_REQUISITION_INVALID_STATUS";
    public const string RequisitionClosedCannotModify = "HR_REC_REQUISITION_CLOSED_CANNOT_MODIFY";
    public const string RequisitionNotApprovedForPosting = "HR_REC_REQUISITION_NOT_APPROVED_FOR_POSTING";
    public const string RequisitionRejectionReasonRequired = "HR_REC_REQUISITION_REJECTION_REASON_REQUIRED";
    public const string RequisitionCancellationReasonRequired = "HR_REC_REQUISITION_CANCELLATION_REASON_REQUIRED";
    public const string RequisitionHeadcountInvalid = "HR_REC_REQUISITION_HEADCOUNT_INVALID";
    public const string RequisitionDateRangeInvalid = "HR_REC_REQUISITION_DATE_RANGE_INVALID";
    public const string RequisitionConcurrencyConflict = "HR_REC_REQUISITION_CONCURRENCY_CONFLICT";
    public const string RecruitmentRequestNotFound = "HR_REC_REQUEST_NOT_FOUND";
    public const string RecruitmentRequestInvalidStatus = "HR_REC_REQUEST_INVALID_STATUS";
    public const string RecruitmentRequestHeadcountInvalid = "HR_REC_REQUEST_HEADCOUNT_INVALID";
    public const string RecruitmentRequestAlreadySubmitted = "HR_REC_REQUEST_ALREADY_SUBMITTED";
    public const string RecruitmentRequestNotApproved = "HR_REC_REQUEST_NOT_APPROVED";
    public const string RecruitmentOpeningNotFound = "HR_REC_OPENING_NOT_FOUND";
    public const string RecruitmentOpeningExceedsPlanned = "HR_REC_OPENING_EXCEEDS_PLANNED";

    public const string JobPostingNotFound = "HR_REC_POSTING_NOT_FOUND";
    public const string JobPostingInvalidStatus = "HR_REC_POSTING_INVALID_STATUS";
    public const string JobPostingDateRangeInvalid = "HR_REC_POSTING_DATE_RANGE_INVALID";

    public const string CandidateNotFound = "HR_REC_CANDIDATE_NOT_FOUND";
    public const string CandidateEmailAlreadyExists = "HR_REC_CANDIDATE_EMAIL_ALREADY_EXISTS";
    public const string CandidatePhoneAlreadyExists = "HR_REC_CANDIDATE_PHONE_ALREADY_EXISTS";
    public const string CandidateBlacklisted = "HR_REC_CANDIDATE_BLACKLISTED";
    public const string CandidateCodeAlreadyExists = "HR_REC_CANDIDATE_CODE_ALREADY_EXISTS";

    public const string ApplicationNotFound = "HR_REC_APPLICATION_NOT_FOUND";
    public const string ApplicationDuplicate = "HR_REC_APPLICATION_DUPLICATE";
    public const string ApplicationInvalidStage = "HR_REC_APPLICATION_INVALID_STAGE";
    public const string ApplicationInvalidStageTransition = "HR_REC_APPLICATION_INVALID_STAGE_TRANSITION";
    public const string ApplicationTerminalStage = "HR_REC_APPLICATION_TERMINAL_STAGE";
    public const string ApplicationAlreadyHired = "HR_REC_APPLICATION_ALREADY_HIRED";
    public const string ApplicationRejectedCannotInterview = "HR_REC_APPLICATION_REJECTED_CANNOT_INTERVIEW";
    public const string CandidateNotActive = "HR_REC_CANDIDATE_NOT_ACTIVE";
    public const string PostingNotPublished = "HR_REC_POSTING_NOT_PUBLISHED";

    public const string InterviewNotFound = "HR_REC_INTERVIEW_NOT_FOUND";
    public const string InterviewDurationInvalid = "HR_REC_INTERVIEW_DURATION_INVALID";
    public const string InterviewerNotFound = "HR_REC_INTERVIEWER_NOT_FOUND";
    public const string InterviewAlreadyCompleted = "HR_REC_INTERVIEW_ALREADY_COMPLETED";
    public const string InterviewFeedbackAlreadyExists = "HR_REC_INTERVIEW_FEEDBACK_EXISTS";
    public const string InvalidApplicationStage = "HR_REC_INVALID_APPLICATION_STAGE";

    public const string HiringDecisionNotFound = "HR_REC_HIRING_DECISION_NOT_FOUND";
    public const string HiringDecisionAlreadyExists = "HR_REC_HIRING_DECISION_ALREADY_EXISTS";
    public const string HiringAlreadyConverted = "HR_REC_HIRING_ALREADY_CONVERTED";
    public const string HireRequiresOfferStage = "HR_REC_HIRE_REQUIRES_OFFER_STAGE";
    public const string ConversionRequiresHireDecision = "HR_REC_CONVERSION_REQUIRES_HIRE_DECISION";
    public const string ConversionRequiresHiredStage = "HR_REC_CONVERSION_REQUIRES_HIRED_STAGE";
    public const string ConversionInvalidCandidateStatus = "HR_REC_CONVERSION_INVALID_CANDIDATE_STATUS";
    public const string ConversionEmployeeCreationFailed = "HR_REC_CONVERSION_EMPLOYEE_CREATION_FAILED";
    public const string HireRequiresFeedback = "HR_REC_HIRE_REQUIRES_FEEDBACK";
    public const string OfferRequiresCompletedInterview = "HR_REC_OFFER_REQUIRES_COMPLETED_INTERVIEW";
    public const string CandidateAlreadyLinked = "HR_REC_CANDIDATE_ALREADY_LINKED";

    // Validation error codes (Recruitment)
    public const string ValRequisitionIdRequired = "VAL_REQUISITION_ID_REQUIRED";
    public const string ValRequisitionCodeRequired = "VAL_REQUISITION_CODE_REQUIRED";
    public const string ValRequisitionCodeTooLong = "VAL_REQUISITION_CODE_TOO_LONG";
    public const string ValRequisitionTitleRequired = "VAL_REQUISITION_TITLE_REQUIRED";
    public const string ValRequisitionHeadcountPositive = "VAL_REQUISITION_HEADCOUNT_POSITIVE";
    public const string ValRequisitionEmploymentTypeRequired = "VAL_REQUISITION_EMPLOYMENT_TYPE_REQUIRED";
    public const string ValRequisitionOpenDateRequired = "VAL_REQUISITION_OPEN_DATE_REQUIRED";
    public const string ValRequisitionTargetHireDateRequired = "VAL_REQUISITION_TARGET_HIRE_DATE_REQUIRED";
    public const string ValPostingIdRequired = "VAL_POSTING_ID_REQUIRED";
    public const string ValPostingTitleRequired = "VAL_POSTING_TITLE_REQUIRED";
    public const string ValCandidateIdRequired = "VAL_CANDIDATE_ID_REQUIRED";
    public const string ValCandidateNameRequired = "VAL_CANDIDATE_NAME_REQUIRED";
    public const string ValCandidateEmailRequired = "VAL_CANDIDATE_EMAIL_REQUIRED";
    public const string ValCandidatePhoneRequired = "VAL_CANDIDATE_PHONE_REQUIRED";
    public const string ValApplicationIdRequired = "VAL_APPLICATION_ID_REQUIRED";
    public const string ValInterviewIdRequired = "VAL_INTERVIEW_ID_REQUIRED";
    public const string ValHiringDecisionIdRequired = "VAL_HIRING_DECISION_ID_REQUIRED";
    public const string ValRecruitmentRequestIdRequired = "VAL_RECRUITMENT_REQUEST_ID_REQUIRED";
    public const string ValRecruitmentRequestDepartmentRequired = "VAL_RECRUITMENT_REQUEST_DEPARTMENT_REQUIRED";
    public const string ValRecruitmentRequestPositionRequired = "VAL_RECRUITMENT_REQUEST_POSITION_REQUIRED";
    public const string ValRecruitmentRequestHeadcountPositive = "VAL_RECRUITMENT_REQUEST_HEADCOUNT_POSITIVE";
    public const string ValRecruitmentRequestPriorityRequired = "VAL_RECRUITMENT_REQUEST_PRIORITY_REQUIRED";

    // Validation error codes (Onboarding)
    public const string ValTemplateIdRequired = "VAL_TEMPLATE_ID_REQUIRED";
    public const string ValTaskIdRequired = "VAL_TASK_ID_REQUIRED";
    public const string ValInstanceIdRequired = "VAL_INSTANCE_ID_REQUIRED";
    public const string ValNewUserIdRequired = "VAL_NEW_USER_ID_REQUIRED";
    public const string ValReasonRequired = "VAL_REASON_REQUIRED";

    // Onboarding error codes
    public const string HrOnboardingTemplateNotFound = "HR_ONB_TEMPLATE_NOT_FOUND";
    public const string HrOnboardingTemplateInactive = "HR_ONB_TEMPLATE_INACTIVE";
    public const string HrOnboardingTemplateInUse = "HR_ONB_TEMPLATE_IN_USE";
    public const string HrOnboardingInstanceNotFound = "HR_ONB_INSTANCE_NOT_FOUND";
    public const string HrOnboardingInstanceInvalidStatus = "HR_ONB_INSTANCE_INVALID_STATUS";
    public const string HrOnboardingInstanceAlreadyCompleted = "HR_ONB_INSTANCE_ALREADY_COMPLETED";
    public const string HrOnboardingInstanceAlreadyCancelled = "HR_ONB_INSTANCE_ALREADY_CANCELLED";
    public const string HrOnboardingInstanceNotInProgress = "HR_ONB_INSTANCE_NOT_IN_PROGRESS";
    public const string HrOnboardingInstanceForceCompleteRequiresReason = "HR_ONB_INSTANCE_FORCE_COMPLETE_REQUIRES_REASON";
    public const string HrOnboardingTaskNotFound = "HR_ONB_TASK_NOT_FOUND";
    public const string HrOnboardingTaskInvalidStatus = "HR_ONB_TASK_INVALID_STATUS";
    public const string HrOnboardingTaskAlreadyCompleted = "HR_ONB_TASK_ALREADY_COMPLETED";
    public const string HrOnboardingTaskAlreadySkipped = "HR_ONB_TASK_ALREADY_SKIPPED";
    public const string HrOnboardingEmployeeAlreadyOnboarding = "HR_ONB_EMPLOYEE_ALREADY_ONBOARDING";
    public const string HrOnboardingEmployeeNotFound = "HR_ONB_EMPLOYEE_NOT_FOUND";
    public const string HrOnboardingResolveRoleMissing = "HR_ONB_RESOLVE_ROLE_MISSING";
    public const string HrOnboardingTemplateHasNoTasks = "HR_ONB_TEMPLATE_HAS_NO_TASKS";
    public const string HrOnboardingTaskAlreadyReopened = "HR_ONB_TASK_ALREADY_REOPENED";
}
