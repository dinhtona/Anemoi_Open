using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Helpers;
using Anemoi.Hr.Application.Abstractions;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Domain.Workflow;
using Anemoi.Hr.ModelIds.ModelIds;
using Microsoft.EntityFrameworkCore;
using OneOf;

namespace Anemoi.Hr.Application.Services;

public sealed class WorkflowBuilder(
    ISqlRepository<WorkflowDefinition> definitionRepository,
    IWorkflowHierarchyResolver hierarchyResolver)
    : IWorkflowBuilder
{
    public async Task<OneOf<WorkflowBuildResult, WorkflowBuildError>> BuildAsync(
        string entityType, EmployeeId requesterEmployeeId, string startedBy, CancellationToken ct)
    {
        var definition = await definitionRepository.GetQueryable()
            .Include(d => d.Steps)
            .Where(d => d.TargetEntityType == entityType && d.IsActive)
            .FirstOrDefaultAsync(ct);

        if (definition is not null)
        {
            var sortedSteps = definition.Steps.OrderBy(s => s.Sequence).ToList();
            return BuildFromDefinition(sortedSteps, definition.Id, definition.Name, definition.Version);
        }

        if (WorkflowConstants.DefaultPolicy.RequiresDefinition(entityType))
            return new WorkflowBuildError("HR_WF_DEF_REQUIRES_DEFINITION");

        return await BuildFromHierarchyAsync(entityType, requesterEmployeeId, ct);
    }

    private static OneOf<WorkflowBuildResult, WorkflowBuildError> BuildFromDefinition(
        List<WorkflowDefinitionStep> steps, WorkflowDefinitionId definitionId,
        string definitionName, int definitionVersion)
    {
        var instanceSteps = steps.Select(s =>
        {
            var stepId = new WorkflowInstanceStepId(IdGenerator.NextGuid());
            return WorkflowInstanceStep.Create(stepId, default, s.Sequence,
                s.ApproverType, s.ApproverValue, null);
        }).ToList();
        return OneOf<WorkflowBuildResult, WorkflowBuildError>.FromT0(
            new WorkflowBuildResult(instanceSteps, definitionId, definitionName, definitionVersion));
    }

    /// <summary>
    /// PREVIEW-ONLY — Hierarchy-based workflow building is NOT used in
    /// production. All required business workflow types must have an
    /// active WorkflowDefinition (see ADR-029). This path is preserved
    /// only for development/testing scenarios where no definition exists.
    /// It must not be relied upon for production workflow routing.
    /// </summary>
    private async Task<OneOf<WorkflowBuildResult, WorkflowBuildError>> BuildFromHierarchyAsync(
        string entityType, EmployeeId requesterEmployeeId, CancellationToken ct)
    {
        var hierarchy = await hierarchyResolver.ResolveHierarchyAsync(requesterEmployeeId, ct);
        var maxSteps = WorkflowConstants.DefaultPolicy.GetStepCount(entityType);
        var selected = hierarchy.Take(maxSteps).ToList();

        var result = selected.Select(s =>
        {
            var stepId = new WorkflowInstanceStepId(IdGenerator.NextGuid());
            return WorkflowInstanceStep.Create(stepId, default, s.StepOrder,
                s.ApproverType, null, null);
        }).ToList();
        return OneOf<WorkflowBuildResult, WorkflowBuildError>.FromT0(
            new WorkflowBuildResult(result, null, null, null));
    }
}
