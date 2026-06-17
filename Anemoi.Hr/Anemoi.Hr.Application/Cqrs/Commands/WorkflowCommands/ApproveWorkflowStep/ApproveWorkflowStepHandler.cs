using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Abstractions;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Workflow;
using Anemoi.Hr.ModelIds.ModelIds;
using Microsoft.EntityFrameworkCore;
using OneOf;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Anemoi.Hr.Application.Cqrs.Commands.WorkflowCommands.ApproveWorkflowStep;

public sealed class ApproveWorkflowStepHandler(
    ISqlRepository<WorkflowInstance> instanceRepository,
    ISqlRepository<WorkflowHistory> historyRepository,
    IUnitOfWork unitOfWork,
    WorkflowMapper mapper,
    IUserRolePermissionService permissionService)
    : ICommandHandler<ApproveWorkflowStepCommand, OneOf<WorkflowInstanceResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<WorkflowInstanceResponse, ErrorDetailResponse>> Handle(
        ApproveWorkflowStepCommand request, CancellationToken cancellationToken)
    {
        var instanceId = new WorkflowInstanceId(Guid.Parse(request.InstanceId));
        var instance = await instanceRepository.GetQueryable()
            .Include(x => x.Steps)
            .Include(x => x.Histories)
            .FirstOrDefaultAsync(x => x.Id == instanceId, cancellationToken);

        if (instance is null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.WorkflowInstanceNotFound);

        try
        {
            if (!instance.IsCurrentStepApprover(
                    request.PerformedBy,
                    role => permissionService.UserHasRole(request.PerformedBy, role),
                    perm => permissionService.UserHasPermission(request.PerformedBy, perm)))
                return HrErrorResponses.Create(HrBusinessErrorCodes.WorkflowInstanceNotApprover);

            var history = instance.Approve(request.PerformedBy, request.Comment);

            var historyCreateResult = await historyRepository.CreateOneAsync(history, cancellationToken);
            if (historyCreateResult.TryPickT1(out var exception, out _))
                return HrErrorResponses.FromSaveResult(exception, HrBusinessErrorCodes.SaveChangesFailed);
        }
        catch (InvalidOperationException)
        {
            return HrErrorResponses.Create(HrBusinessErrorCodes.WorkflowInstanceInvalidStatus);
        }

        var saveResult = await unitOfWork.SaveChangesAsync(cancellationToken);
        if (saveResult.TryPickT1(out var saveException, out _))
            return HrErrorResponses.FromSaveResult(saveException, HrBusinessErrorCodes.SaveChangesFailed);

        return mapper.ToResponse(instance);
    }
}
