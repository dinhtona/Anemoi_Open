using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Helpers;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Contract.Identity.ModelIds;
using Anemoi.Hr.Application.Abstractions;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Models;
using Anemoi.Hr.Domain.Employees;
using Anemoi.Hr.Domain.Workflow;
using Anemoi.Hr.ModelIds.ModelIds;
using Microsoft.EntityFrameworkCore;
using OneOf;

namespace Anemoi.Hr.Application.Services;

public sealed class WorkflowEngine(
    ISqlRepository<WorkflowInstance> instanceRepository,
    ISqlRepository<WorkflowHistory> historyRepository,
    ISqlRepository<Employee> employeeRepository,
    IWorkflowBuilder workflowBuilder,
    IApprovalResolver approvalResolver)
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

        var activateResult = await ActivateCurrentStepAsync(instance, ct);
        if (activateResult.TryPickT1(out var error, out _))
            return error;

        return instance;
    }

    public async Task<OneOf<WorkflowInstance, ErrorDetailResponse>> ApproveAsync(
        WorkflowInstanceId workflowInstanceId, UserId performedBy,
        string? comment, CancellationToken ct)
    {
        var instance = await LoadInstanceAsync(workflowInstanceId, ct);
        if (instance is null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.WorkflowInstanceNotFound);

        var step = instance.Steps.FirstOrDefault(s => s.Sequence == instance.CurrentStep);
        if (step is null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.WorkflowInstanceStepNotFound);

        // Activate step if not yet resolved
        if (step.ApproverEmployeeId is null)
        {
            var activateResult = await ActivateCurrentStepAsync(instance, ct);
            if (activateResult.TryPickT1(out var error, out _))
                return error;
        }

        // Check permission: performedBy must match the current step's resolved approver
        var performerEmployee = await employeeRepository.GetQueryable()
            .FirstOrDefaultAsync(e => e.IdentityUserId == performedBy.Value, ct);

        if (performerEmployee is null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.WorkflowInstanceNotApprover);

        if (!instance.IsCurrentStepApprover(performerEmployee.Id))
            return HrErrorResponses.Create(HrBusinessErrorCodes.WorkflowInstanceNotApprover);

        try
        {
            var historyId = new WorkflowHistoryId(IdGenerator.NextGuid());
            var history = instance.Approve(historyId, performedBy.Value.ToString(), comment);
            await historyRepository.CreateOneAsync(history, ct);
        }
        catch (InvalidOperationException)
        {
            return HrErrorResponses.Create(HrBusinessErrorCodes.WorkflowInstanceInvalidStatus);
        }

        // Resolve next step's approver
        if (instance.Status == WorkflowStatusCode.Pending)
        {
            var nextStep = instance.Steps.FirstOrDefault(s => s.Status == WorkflowStepStatusCode.Pending);
            if (nextStep is not null)
            {
                var activateResult = await ActivateCurrentStepAsync(instance, ct);
                if (activateResult.TryPickT1(out _, out _))
                {
                    // Next step activation failed — workflow continues but step has no approver
                }
            }
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

        var step = instance.Steps.FirstOrDefault(s => s.Sequence == instance.CurrentStep);
        if (step is null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.WorkflowInstanceStepNotFound);

        if (step.ApproverEmployeeId is null)
        {
            var activateResult = await ActivateCurrentStepAsync(instance, ct);
            if (activateResult.TryPickT1(out var error, out _))
                return error;
        }

        var performerEmployee = await employeeRepository.GetQueryable()
            .FirstOrDefaultAsync(e => e.IdentityUserId == performedBy.Value, ct);

        if (performerEmployee is null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.WorkflowInstanceNotApprover);

        if (!instance.IsCurrentStepApprover(performerEmployee.Id))
            return HrErrorResponses.Create(HrBusinessErrorCodes.WorkflowInstanceNotApprover);

        try
        {
            var historyId = new WorkflowHistoryId(IdGenerator.NextGuid());
            var history = instance.Reject(historyId, performedBy.Value.ToString(), comment);
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
            var historyId = new WorkflowHistoryId(IdGenerator.NextGuid());
            var history = instance.Cancel(historyId, performedBy.Value.ToString());
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

        if (step.ApproverUserId is not null)
            return [new UserId(Guid.Parse(step.ApproverUserId))];

        return [];
    }

    public async Task<IReadOnlyList<WorkflowApproverCandidate>> ResolveApproversAsync(
        WorkflowInstanceId workflowInstanceId, CancellationToken ct)
    {
        var instance = await LoadInstanceAsync(workflowInstanceId, ct);
        if (instance is null) return [];

        var context = new ApprovalRoutingContext(
            instance.RequesterEmployeeId, null, null,
            instance.EntityType, instance.EntityId);

        var candidates = new List<WorkflowApproverCandidate>();
        foreach (var step in instance.Steps.OrderBy(s => s.Sequence))
        {
            if (step.ApproverEmployeeId is not null)
            {
                var employee = await employeeRepository.GetQueryable()
                    .FirstOrDefaultAsync(e => e.Id == step.ApproverEmployeeId, ct);
                candidates.Add(new WorkflowApproverCandidate(
                    step.Sequence,
                    step.ApproverTypeSnapshot,
                    step.ApproverValueSnapshot,
                    step.ApproverEmployeeId,
                    employee?.FullName,
                    employee?.WorkEmail));
                continue;
            }

            var result = await approvalResolver.ResolveApproversAsync(
                step.ApproverTypeSnapshot, step.ApproverValueSnapshot, context, ct);

            if (result.TryPickT0(out var approvers, out _) && approvers.Count > 0)
            {
                candidates.Add(new WorkflowApproverCandidate(
                    step.Sequence,
                    step.ApproverTypeSnapshot,
                    step.ApproverValueSnapshot,
                    approvers[0].EmployeeId,
                    approvers[0].FullName,
                    approvers[0].Email));
            }
            else
            {
                candidates.Add(new WorkflowApproverCandidate(
                    step.Sequence,
                    step.ApproverTypeSnapshot,
                    step.ApproverValueSnapshot,
                    null, null, null));
            }
        }

        return candidates;
    }

    private async Task<OneOf<bool, ErrorDetailResponse>> ActivateCurrentStepAsync(
        WorkflowInstance instance, CancellationToken ct)
    {
        var step = instance.Steps.FirstOrDefault(s => s.Sequence == instance.CurrentStep);
        if (step is null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.WorkflowInstanceStepNotFound);

        if (step.ApproverEmployeeId is not null)
            return true;

        var context = new ApprovalRoutingContext(
            instance.RequesterEmployeeId, null, null,
            instance.EntityType, instance.EntityId);

        var result = await approvalResolver.ResolveApproversAsync(
            step.ApproverTypeSnapshot, step.ApproverValueSnapshot, context, ct);

        if (result.TryPickT1(out var error, out _))
            return error;

        var approvers = result.AsT0;
        if (approvers.Count == 0)
            return HrErrorResponses.Create(HrBusinessErrorCodes.WorkflowApproverNotFound);

        step.SetApprover(approvers[0].EmployeeId, approvers[0].UserId.Value.ToString());
        return true;
    }

    private async Task<WorkflowInstance?> LoadInstanceAsync(WorkflowInstanceId id, CancellationToken ct)
    {
        return await instanceRepository.GetQueryable()
            .Include(x => x.Steps)
            .Include(x => x.Histories)
            .FirstOrDefaultAsync(x => x.Id == id, ct);
    }
}
