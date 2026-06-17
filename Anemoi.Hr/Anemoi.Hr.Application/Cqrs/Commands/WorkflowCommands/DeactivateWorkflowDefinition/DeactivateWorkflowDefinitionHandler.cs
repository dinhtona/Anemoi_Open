using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Workflow;
using Anemoi.Hr.ModelIds.ModelIds;
using Microsoft.EntityFrameworkCore;
using OneOf;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Anemoi.Hr.Application.Cqrs.Commands.WorkflowCommands.DeactivateWorkflowDefinition;

public sealed class DeactivateWorkflowDefinitionHandler(
    ISqlRepository<WorkflowDefinition> repository,
    IUnitOfWork unitOfWork,
    WorkflowMapper mapper)
    : ICommandHandler<DeactivateWorkflowDefinitionCommand, OneOf<WorkflowDefinitionResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<WorkflowDefinitionResponse, ErrorDetailResponse>> Handle(
        DeactivateWorkflowDefinitionCommand request, CancellationToken cancellationToken)
    {
        var id = new WorkflowDefinitionId(Guid.Parse(request.Id));
        var definition = await repository.GetQueryable()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (definition is null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.WorkflowDefinitionNotFound);

        if (!definition.IsActive)
            return HrErrorResponses.Create(HrBusinessErrorCodes.WorkflowDefinitionAlreadyInactive);

        definition.Deactivate();

        var saveResult = await unitOfWork.SaveChangesAsync(cancellationToken);
        if (saveResult.TryPickT1(out var saveException, out _))
            return HrErrorResponses.FromSaveResult(saveException, HrBusinessErrorCodes.SaveChangesFailed);

        return mapper.ToResponse(definition);
    }
}
