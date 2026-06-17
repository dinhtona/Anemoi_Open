using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.BuildingBlock.Application.Helpers;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Contract.Identity.ModelIds;
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

namespace Anemoi.Hr.Application.Cqrs.Commands.WorkflowCommands.StartWorkflow;

public sealed class StartWorkflowHandler(
    ISqlRepository<WorkflowDefinition> definitionRepository,
    ISqlRepository<WorkflowInstance> instanceRepository,
    IUnitOfWork unitOfWork,
    WorkflowMapper mapper)
    : ICommandHandler<StartWorkflowCommand, OneOf<WorkflowInstanceResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<WorkflowInstanceResponse, ErrorDetailResponse>> Handle(
        StartWorkflowCommand request, CancellationToken cancellationToken)
    {
        var definitionId = new WorkflowDefinitionId(Guid.Parse(request.DefinitionId));
        var definition = await definitionRepository.GetQueryable()
            .Include(x => x.Steps.OrderBy(s => s.Sequence))
            .FirstOrDefaultAsync(x => x.Id == definitionId, cancellationToken);

        if (definition is null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.WorkflowDefinitionNotFound);

        if (!definition.IsActive)
            return HrErrorResponses.Create(HrBusinessErrorCodes.WorkflowDefinitionNoSteps);

        var instanceId = new WorkflowInstanceId(IdGenerator.NextGuid());

        var instanceSteps = definition.Steps.Select(s =>
        {
            var stepId = new WorkflowInstanceStepId(IdGenerator.NextGuid());
            string? approverUserId = s.ApproverType switch
            {
                ApproverType.SpecificUser => s.ApproverValue,
                ApproverType.DirectManager => ResolveDirectManager(request.StartedBy),
                _ => null
            };
            return WorkflowInstanceStep.Create(stepId, instanceId, s.Sequence,
                s.ApproverType, s.ApproverValue, approverUserId);
        }).ToList();

        var instance = WorkflowInstance.Start(instanceId, definitionId,
            request.EntityType, request.EntityId, request.StartedBy,
            new EmployeeId(Guid.Parse(request.RequesterEmployeeId)),
            new UserId(Guid.Parse(request.RequesterUserId)),
            instanceSteps);

        var createResult = await instanceRepository.CreateOneAsync(instance, cancellationToken);
        if (createResult.TryPickT1(out var exception, out _))
            return HrErrorResponses.FromSaveResult(exception, HrBusinessErrorCodes.SaveChangesFailed);

        var saveResult = await unitOfWork.SaveChangesAsync(cancellationToken);
        if (saveResult.TryPickT1(out var saveException, out _))
            return HrErrorResponses.FromSaveResult(saveException, HrBusinessErrorCodes.SaveChangesFailed);

        return mapper.ToResponse(instance, definition.Name);
    }

    private static string? ResolveDirectManager(string userId)
    {
        // Phase 28: placeholder — will be implemented when integrated with employee data
        return null;
    }
}
