using System;

namespace Anemoi.Hr.Application.Configurations;

public static class WorkflowConstants
{
    public static class TargetEntityTypes
    {
        public const string LeaveRequest = "LeaveRequest";
        public const string OvertimeRequest = "OvertimeRequest";
        public const string PayrollRun = "PayrollRun";
        public const string RecruitmentRequest = "RecruitmentRequest";
    }

    public static class DefaultPolicy
    {
        public const int LeaveRequestSteps = 2;
        public const int OvertimeRequestSteps = 2;
        public const int RecruitmentRequestSteps = 0;
        public const int PayrollRunSteps = 0;

        public static int GetStepCount(string entityType) => entityType switch
        {
            TargetEntityTypes.LeaveRequest => LeaveRequestSteps,
            TargetEntityTypes.OvertimeRequest => OvertimeRequestSteps,
            TargetEntityTypes.RecruitmentRequest => RecruitmentRequestSteps,
            TargetEntityTypes.PayrollRun => PayrollRunSteps,
            _ => throw new InvalidOperationException($"No default workflow policy for '{entityType}'.")
        };

        public static bool RequiresDefinition(string entityType) => GetStepCount(entityType) == 0;
    }
}
