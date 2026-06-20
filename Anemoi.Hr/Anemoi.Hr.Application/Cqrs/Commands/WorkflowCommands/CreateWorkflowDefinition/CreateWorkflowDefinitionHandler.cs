using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.BuildingBlock.Application.Helpers;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Workflow;
using Anemoi.Hr.ModelIds.ModelIds;
using OneOf;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Anemoi.Hr.Application.Cqrs.Commands.WorkflowCommands.CreateWorkflowDefinition;

public sealed class CreateWorkflowDefinitionHandler(
    ISqlRepository<WorkflowDefinition> repository,
    IUnitOfWork unitOfWork,
    WorkflowMapper mapper)
    : ICommandHandler<CreateWorkflowDefinitionCommand, OneOf<WorkflowDefinitionResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<WorkflowDefinitionResponse, ErrorDetailResponse>> Handle(
        CreateWorkflowDefinitionCommand request, CancellationToken cancellationToken)
    {
        var id = new WorkflowDefinitionId(IdGenerator.NextGuid());
        var steps = request.Steps.Select(s =>
        {
            var stepId = new WorkflowDefinitionStepId(IdGenerator.NextGuid());
            return WorkflowDefinitionStep.Create(stepId, id, s.Sequence, s.ApproverType, s.ApproverValue, s.IsRequired);
        }).ToList();

        var definition = WorkflowDefinition.Create(id, request.Code, request.Name, request.Description, request.WorkflowTypeCode, request.WorkflowTypeCode, 1, steps);

        var createResult = await repository.CreateOneAsync(definition, cancellationToken);
        if (createResult.TryPickT1(out var exception, out _))
            return HrErrorResponses.FromSaveResult(exception, HrBusinessErrorCodes.SaveChangesFailed);

        var saveResult = await unitOfWork.SaveChangesAsync(cancellationToken);
        if (saveResult.TryPickT1(out var saveException, out _))
            return HrErrorResponses.FromSaveResult(saveException, HrBusinessErrorCodes.SaveChangesFailed);

        return mapper.ToResponse(definition);
    }
}
