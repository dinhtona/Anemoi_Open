using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Contract.Hr.Events;
using Anemoi.Hr.Application.Abstractions;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Domain.Overtime;
using Anemoi.Hr.ModelIds.ModelIds;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using OneOf;

namespace Anemoi.Hr.Application.WorkflowTargetStatusUpdaters;

public sealed class OvertimeWorkflowStatusUpdater(
    ISqlRepository<OvertimeRequest> overtimeRepository,
    IUnitOfWork unitOfWork,
    IPublishEndpoint publishEndpoint)
    : IWorkflowTargetStatusUpdater
{
    public bool CanHandle(string entityType)
        => entityType == WorkflowConstants.TargetEntityTypes.OvertimeRequest;

    public async Task<OneOf<bool, ErrorDetailResponse>> MarkApprovedAsync(string entityId, string performedBy, CancellationToken ct)
    {
        var id = new OvertimeRequestId(Guid.Parse(entityId));
        var overtime = await overtimeRepository.GetQueryable()
            .FirstOrDefaultAsync(x => x.Id == id, ct);
        if (overtime is null) return true;
        overtime.Approve(performedBy);
        await publishEndpoint.Publish(new OvertimeRequestApprovedIntegrationEvent(
            overtime.Id.Value.ToString(),
            overtime.EmployeeId.Value.ToString()), ct);
        await unitOfWork.SaveChangesAsync(ct);
        return true;
    }

    public async Task<OneOf<bool, ErrorDetailResponse>> MarkRejectedAsync(string entityId, string performedBy, string? reason, CancellationToken ct)
    {
        var id = new OvertimeRequestId(Guid.Parse(entityId));
        var overtime = await overtimeRepository.GetQueryable()
            .FirstOrDefaultAsync(x => x.Id == id, ct);
        if (overtime is null) return true;
        overtime.Reject(performedBy, reason);
        await publishEndpoint.Publish(new OvertimeRequestRejectedIntegrationEvent(
            overtime.Id.Value.ToString(),
            overtime.EmployeeId.Value.ToString()), ct);
        await unitOfWork.SaveChangesAsync(ct);
        return true;
    }
}
