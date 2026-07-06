using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Contract.Identity.ModelIds;
using Anemoi.Hr.Application.Abstractions;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;
using OneOf;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Anemoi.Hr.Application.Cqrs.Commands.WorkflowCommands.RejectWorkflowStep;

public sealed class RejectWorkflowStepHandler(
    IWorkflowEngine workflowEngine,
    IUnitOfWork unitOfWork,
    WorkflowMapper mapper)
    : ICommandHandler<RejectWorkflowStepCommand, OneOf<WorkflowInstanceResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<WorkflowInstanceResponse, ErrorDetailResponse>> Handle(
        RejectWorkflowStepCommand request, CancellationToken cancellationToken)
    {
        var engineResult = await workflowEngine.RejectAsync(
            new WorkflowInstanceId(Guid.Parse(request.InstanceId)),
            new UserId(Guid.Parse(request.PerformedBy)),
            request.Comment,
            cancellationToken);

        if (engineResult.TryPickT1(out var error, out var instance))
            return error;

        var saveResult = await unitOfWork.SaveChangesAsync(cancellationToken);
        if (saveResult.TryPickT1(out var saveException, out _))
            return HrErrorResponses.FromSaveResult(saveException, HrBusinessErrorCodes.SaveChangesFailed);

        return mapper.ToResponse(instance);
    }
}
