using System;
using System.Collections.Generic;

namespace Anemoi.Hr.Application.Configurations;

public static class WorkflowConstants
{
    public static class TargetEntityTypes
    {
        public const string LeaveRequest = "LeaveRequest";
        public const string OvertimeRequest = "OvertimeRequest";
        public const string PayrollRun = "PayrollRun";
        public const string RecruitmentRequest = "RecruitmentRequest";
        public const string EmployeeTransfer = "EmployeeTransfer";
        public const string EmployeeSeparation = "EmployeeSeparation";
        public const string ProbationRecord = "ProbationRecord";
    }

    private static readonly HashSet<string> RequiredEntityTypes =
    [
        TargetEntityTypes.LeaveRequest,
        TargetEntityTypes.OvertimeRequest,
        TargetEntityTypes.PayrollRun,
        TargetEntityTypes.RecruitmentRequest,
        TargetEntityTypes.EmployeeTransfer,
        TargetEntityTypes.EmployeeSeparation,
        TargetEntityTypes.ProbationRecord
    ];

    public static bool IsRequiredEntityType(string entityType) => RequiredEntityTypes.Contains(entityType);

    public static class DefaultPolicy
    {
        public const int LeaveRequestSteps = 2;
        public const int OvertimeRequestSteps = 2;
        public const int RecruitmentRequestSteps = 2;
        public const int PayrollRunSteps = 2;
        public const int EmployeeTransferSteps = 2;
        public const int EmployeeSeparationSteps = 2;
        public const int ProbationRecordSteps = 2;

        public static int GetStepCount(string entityType) => entityType switch
        {
            TargetEntityTypes.LeaveRequest => LeaveRequestSteps,
            TargetEntityTypes.OvertimeRequest => OvertimeRequestSteps,
            TargetEntityTypes.RecruitmentRequest => RecruitmentRequestSteps,
            TargetEntityTypes.PayrollRun => PayrollRunSteps,
            TargetEntityTypes.EmployeeTransfer => EmployeeTransferSteps,
            TargetEntityTypes.EmployeeSeparation => EmployeeSeparationSteps,
            TargetEntityTypes.ProbationRecord => ProbationRecordSteps,
            _ => throw new InvalidOperationException($"No default workflow policy for '{entityType}'.")
        };

        /// <summary>
        /// All required business workflow types are definition-bound.
        /// Hierarchy fallback (BuildFromHierarchyAsync) is NOT allowed for
        /// production business workflows. Every entity type listed in
        /// RequiredEntityTypes must have an active WorkflowDefinition
        /// before submissions can succeed. This is by design — see ADR-029
        /// in ARCHITECTURE_DECISIONS.md.
        /// </summary>
        public static bool RequiresDefinition(string entityType) => IsRequiredEntityType(entityType);
    }
}
