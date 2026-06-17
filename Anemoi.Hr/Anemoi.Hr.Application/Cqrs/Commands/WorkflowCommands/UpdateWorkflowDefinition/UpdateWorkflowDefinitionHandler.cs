using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.BuildingBlock.Application.Helpers;
using Anemoi.BuildingBlock.Application.Responses;
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

namespace Anemoi.Hr.Application.Cqrs.Commands.WorkflowCommands.UpdateWorkflowDefinition;

public sealed class UpdateWorkflowDefinitionHandler(
    ISqlRepository<WorkflowDefinition> repository,
    IUnitOfWork unitOfWork,
    WorkflowMapper mapper)
    : ICommandHandler<UpdateWorkflowDefinitionCommand, OneOf<WorkflowDefinitionResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<WorkflowDefinitionResponse, ErrorDetailResponse>> Handle(
        UpdateWorkflowDefinitionCommand request, CancellationToken cancellationToken)
    {
        var id = new WorkflowDefinitionId(Guid.Parse(request.Id));
        var definition = await repository.GetQueryable()
            .Include(x => x.Steps)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (definition is null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.WorkflowDefinitionNotFound);

        if (definition.IsActive)
            return HrErrorResponses.Create(HrBusinessErrorCodes.WorkflowDefinitionActiveCannotUpdate);

        definition.UpdateDetails(request.Name, request.Description);

        var newSteps = request.Steps.Select(s =>
        {
            var stepId = new WorkflowDefinitionStepId(IdGenerator.NextGuid());
            return WorkflowDefinitionStep.Create(stepId, id, s.Sequence, s.ApproverType, s.ApproverValue, s.IsRequired);
        }).ToList();

        definition.ReplaceSteps(newSteps);

        var saveResult = await unitOfWork.SaveChangesAsync(cancellationToken);
        if (saveResult.TryPickT1(out var saveException, out _))
            return HrErrorResponses.FromSaveResult(saveException, HrBusinessErrorCodes.SaveChangesFailed);

        return mapper.ToResponse(definition);
    }
}
