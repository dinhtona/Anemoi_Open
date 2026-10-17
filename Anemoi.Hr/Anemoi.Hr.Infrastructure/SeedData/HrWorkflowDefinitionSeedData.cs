using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Domain.Workflow;
using Anemoi.Hr.ModelIds.ModelIds;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Anemoi.Hr.Infrastructure.SeedData;

/// <summary>
/// FUNC-004 / ADR-029: idempotent seed of the 7 required workflow definitions.
/// Runs in EVERY environment (not gated on Development): ADR-029 makes every
/// required business workflow type definition-bound and forbids hierarchy
/// fallback in production, so each type must have an active WorkflowDefinition
/// before the API serves traffic. Each definition is checked by its stable
/// Code before insert, so re-running is a safe no-op.
/// </summary>
public static class HrWorkflowDefinitionSeedData
{
    private static readonly WorkflowDefinitionId TransferWorkflowDefId =
        new(Guid.Parse("80000000-0000-0000-0000-000000000001"));
    private static readonly WorkflowDefinitionId SeparationWorkflowDefId =
        new(Guid.Parse("80000000-0000-0000-0000-000000000002"));
    private static readonly WorkflowDefinitionId LeaveWorkflowDefId =
        new(Guid.Parse("80000000-0000-0000-0000-000000000003"));
    private static readonly WorkflowDefinitionId OvertimeWorkflowDefId =
        new(Guid.Parse("80000000-0000-0000-0000-000000000004"));
    private static readonly WorkflowDefinitionId PayrollWorkflowDefId =
        new(Guid.Parse("80000000-0000-0000-0000-000000000005"));
    private static readonly WorkflowDefinitionId RecruitmentWorkflowDefId =
        new(Guid.Parse("80000000-0000-0000-0000-000000000006"));
    private static readonly WorkflowDefinitionId ProbationWorkflowDefId =
        new(Guid.Parse("80000000-0000-0000-0000-000000000007"));

    public static async Task SeedAsync(IServiceScope serviceScope, CancellationToken cancellationToken = default)
    {
        var workflowDefinitionRepository = serviceScope.ServiceProvider
            .GetRequiredService<ISqlRepository<WorkflowDefinition>>();
        var unitOfWork = serviceScope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var logger = serviceScope.ServiceProvider.GetRequiredService<ILogger>();

        await SeedDefinitionIfNotExists(workflowDefinitionRepository, cancellationToken,
            "LEAVE-REQUEST", "Leave Request Approval",
            WorkflowConstants.TargetEntityTypes.LeaveRequest,
            LeaveWorkflowDefId, 0x301, 0x302);

        await SeedDefinitionIfNotExists(workflowDefinitionRepository, cancellationToken,
            "OVERTIME-REQUEST", "Overtime Request Approval",
            WorkflowConstants.TargetEntityTypes.OvertimeRequest,
            OvertimeWorkflowDefId, 0x401, 0x402);

        await SeedDefinitionIfNotExists(workflowDefinitionRepository, cancellationToken,
            "PAYROLL-RUN", "Payroll Run Approval",
            WorkflowConstants.TargetEntityTypes.PayrollRun,
            PayrollWorkflowDefId, 0x501, 0x502);

        await SeedDefinitionIfNotExists(workflowDefinitionRepository, cancellationToken,
            "RECRUITMENT-REQUEST", "Recruitment Request Approval",
            WorkflowConstants.TargetEntityTypes.RecruitmentRequest,
            RecruitmentWorkflowDefId, 0x601, 0x602);

        await SeedDefinitionIfNotExists(workflowDefinitionRepository, cancellationToken,
            "EMPLOYEE-TRANSFER", "Employee Transfer Approval",
            WorkflowConstants.TargetEntityTypes.EmployeeTransfer,
            TransferWorkflowDefId, 0x101, 0x102);

        await SeedDefinitionIfNotExists(workflowDefinitionRepository, cancellationToken,
            "EMPLOYEE-SEPARATION", "Employee Separation Approval",
            WorkflowConstants.TargetEntityTypes.EmployeeSeparation,
            SeparationWorkflowDefId, 0x201, 0x202);

        await SeedDefinitionIfNotExists(workflowDefinitionRepository, cancellationToken,
            "PROBATION-RECORD", "Probation Record Approval",
            WorkflowConstants.TargetEntityTypes.ProbationRecord,
            ProbationWorkflowDefId, 0x701, 0x702);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        logger.Information("[SeedData] Required workflow definitions are present (idempotent seed)");
    }

    private static async Task SeedDefinitionIfNotExists(
        ISqlRepository<WorkflowDefinition> repository,
        CancellationToken cancellationToken,
        string code,
        string name,
        string targetEntityType,
        WorkflowDefinitionId defId,
        int step1Suffix,
        int step2Suffix)
    {
        if (await repository.ExistByConditionAsync(
                x => x.Code == code, cancellationToken)) return;

        var steps = new List<WorkflowDefinitionStep>
        {
            WorkflowDefinitionStep.Create(
                new WorkflowDefinitionStepId(Guid.Parse($"80000000-0000-0000-0000-0000{step1Suffix:X8}")),
                defId, 1, ApproverType.DirectManager, null, true),
            WorkflowDefinitionStep.Create(
                new WorkflowDefinitionStepId(Guid.Parse($"80000000-0000-0000-0000-0000{step2Suffix:X8}")),
                defId, 2, ApproverType.HrManager, null, true)
        };

        var def = WorkflowDefinition.Create(
            defId, code, name,
            null, WorkflowTypeCode.Approval, targetEntityType,
            1, steps);
        def.Activate();
        await repository.CreateOneAsync(def, cancellationToken);
    }
}
