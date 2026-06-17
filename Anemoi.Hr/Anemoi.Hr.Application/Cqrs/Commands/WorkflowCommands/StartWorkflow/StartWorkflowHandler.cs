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

namespace Anemoi.Hr.Application.Cqrs.Commands.WorkflowCommands.StartWorkflow;

public sealed class StartWorkflowHandler(
    IWorkflowEngine workflowEngine,
    IUnitOfWork unitOfWork,
    WorkflowMapper mapper)
    : ICommandHandler<StartWorkflowCommand, OneOf<WorkflowInstanceResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<WorkflowInstanceResponse, ErrorDetailResponse>> Handle(
        StartWorkflowCommand request, CancellationToken cancellationToken)
    {
        var engineResult = await workflowEngine.StartAsync(
            request.EntityType,
            Guid.Parse(request.EntityId),
            new EmployeeId(Guid.Parse(request.RequesterEmployeeId)),
            new UserId(Guid.Parse(request.RequesterUserId)),
            new UserId(Guid.Parse(request.StartedBy)),
            cancellationToken);

        if (engineResult.TryPickT1(out var error, out var instance))
            return error;

        var saveResult = await unitOfWork.SaveChangesAsync(cancellationToken);
        if (saveResult.TryPickT1(out var saveException, out _))
            return HrErrorResponses.FromSaveResult(saveException, HrBusinessErrorCodes.SaveChangesFailed);

        return mapper.ToResponse(instance);
    }
}
