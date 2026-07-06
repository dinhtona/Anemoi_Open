namespace Anemoi.Contract.Hr;

public static class NotificationWorkflowConstants
{
    public static class TargetServices
    {
        public const string Leave = "Leave";
        public const string Overtime = "Overtime";
        public const string Payroll = "Payroll";
        public const string Onboarding = "Onboarding";
        public const string Recruitment = "Recruitment";
    }

    public static class ActionCodes
    {
        // Leave
        public const string ViewLeaveRequest = "ViewLeaveRequest";
        public const string ApproveLeaveRequest = "ApproveLeaveRequest";
        public const string RejectLeaveRequest = "RejectLeaveRequest";

        // Overtime
        public const string ViewOvertimeRequest = "ViewOvertimeRequest";
        public const string ApproveOvertimeRequest = "ApproveOvertimeRequest";
        public const string RejectOvertimeRequest = "RejectOvertimeRequest";

        // Payroll
        public const string ViewPayrollRun = "ViewPayrollRun";
        public const string ApprovePayrollRun = "ApprovePayrollRun";
        public const string RejectPayrollRun = "RejectPayrollRun";

        // Onboarding
        public const string ViewOnboardingTask = "ViewOnboardingTask";
        public const string CompleteOnboardingTask = "CompleteOnboardingTask";

        // Recruitment
        public const string ViewRecruitmentRequest = "ViewRecruitmentRequest";
    }
}
