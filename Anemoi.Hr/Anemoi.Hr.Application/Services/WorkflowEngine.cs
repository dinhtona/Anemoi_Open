using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Helpers;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Contract.Identity.ModelIds;
using Anemoi.Hr.Application.Abstractions;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Domain.Workflow;
using Anemoi.Hr.ModelIds.ModelIds;
using Microsoft.EntityFrameworkCore;
using OneOf;

namespace Anemoi.Hr.Application.Services;

public sealed class WorkflowEngine(
    ISqlRepository<WorkflowInstance> instanceRepository,
    ISqlRepository<WorkflowHistory> historyRepository,
    IWorkflowBuilder workflowBuilder)
    : IWorkflowEngine
{
    public async Task<OneOf<WorkflowInstance, ErrorDetailResponse>> StartAsync(
        string entityType, Guid entityId,
        EmployeeId requesterEmployeeId, UserId requesterUserId,
        UserId startedBy, CancellationToken ct)
    {
        var buildResult = await workflowBuilder.BuildAsync(
            entityType, requesterEmployeeId, startedBy.Value.ToString(), ct);

        if (buildResult.TryPickT1(out var buildError, out var steps))
            return HrErrorResponses.Create(buildError.ErrorCode);

        var instanceId = new WorkflowInstanceId(IdGenerator.NextGuid());

        var instance = WorkflowInstance.Start(
            instanceId, null, entityType, entityId.ToString(),
            startedBy.Value.ToString(),
            requesterEmployeeId, requesterUserId,
            steps.ToList());

        var createResult = await instanceRepository.CreateOneAsync(instance, ct);
        if (createResult.TryPickT1(out _, out _))
            return HrErrorResponses.Create(HrBusinessErrorCodes.SaveChangesFailed);

        return instance;
    }

    public async Task<OneOf<WorkflowInstance, ErrorDetailResponse>> ApproveAsync(
        WorkflowInstanceId workflowInstanceId, UserId performedBy,
        string? comment, CancellationToken ct)
    {
        var instance = await LoadInstanceAsync(workflowInstanceId, ct);
        if (instance is null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.WorkflowInstanceNotFound);

        try
        {
            if (!instance.IsCurrentStepApprover(performedBy.Value.ToString(), _ => false, _ => false))
                return HrErrorResponses.Create(HrBusinessErrorCodes.WorkflowInstanceNotApprover);

            var history = instance.Approve(performedBy.Value.ToString(), comment);
            await historyRepository.CreateOneAsync(history, ct);
        }
        catch (InvalidOperationException)
        {
            return HrErrorResponses.Create(HrBusinessErrorCodes.WorkflowInstanceInvalidStatus);
        }

        return instance;
    }

    public async Task<OneOf<WorkflowInstance, ErrorDetailResponse>> RejectAsync(
        WorkflowInstanceId workflowInstanceId, UserId performedBy,
        string? comment, CancellationToken ct)
    {
        var instance = await LoadInstanceAsync(workflowInstanceId, ct);
        if (instance is null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.WorkflowInstanceNotFound);

        try
        {
            if (!instance.IsCurrentStepApprover(performedBy.Value.ToString(), _ => false, _ => false))
                return HrErrorResponses.Create(HrBusinessErrorCodes.WorkflowInstanceNotApprover);

            var history = instance.Reject(performedBy.Value.ToString(), comment);
            await historyRepository.CreateOneAsync(history, ct);
        }
        catch (InvalidOperationException)
        {
            return HrErrorResponses.Create(HrBusinessErrorCodes.WorkflowInstanceInvalidStatus);
        }

        return instance;
    }

    public async Task<OneOf<WorkflowInstance, ErrorDetailResponse>> CancelAsync(
        WorkflowInstanceId workflowInstanceId, UserId performedBy,
        CancellationToken ct)
    {
        var instance = await LoadInstanceAsync(workflowInstanceId, ct);
        if (instance is null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.WorkflowInstanceNotFound);

        try
        {
            var history = instance.Cancel(performedBy.Value.ToString());
            await historyRepository.CreateOneAsync(history, ct);
        }
        catch (InvalidOperationException)
        {
            return HrErrorResponses.Create(HrBusinessErrorCodes.WorkflowInstanceInvalidStatus);
        }

        return instance;
    }

    public async Task<IReadOnlyList<UserId>> GetCurrentApproversAsync(
        WorkflowInstanceId workflowInstanceId, CancellationToken ct)
    {
        var instance = await LoadInstanceAsync(workflowInstanceId, ct);
        if (instance is null) return [];

        var step = instance.Steps.FirstOrDefault(s => s.Sequence == instance.CurrentStep);
        if (step is null) return [];

        return step.ApproverTypeSnapshot switch
        {
            ApproverType.SpecificUser when step.ApproverUserId is not null
                => [new UserId(Guid.Parse(step.ApproverUserId))],
            ApproverType.DirectManager when step.ApproverUserId is not null
                => [new UserId(Guid.Parse(step.ApproverUserId))],
            _ => []
        };
    }

    private async Task<WorkflowInstance?> LoadInstanceAsync(WorkflowInstanceId id, CancellationToken ct)
    {
        return await instanceRepository.GetQueryable()
            .Include(x => x.Steps)
            .Include(x => x.Histories)
            .FirstOrDefaultAsync(x => x.Id == id, ct);
    }
}
