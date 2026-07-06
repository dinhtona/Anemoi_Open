using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.BuildingBlock.Application.Helpers;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Onboarding;
using Anemoi.Hr.ModelIds.ModelIds;
using OneOf;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Anemoi.Hr.Application.Cqrs.Commands.OnboardingCommands.CreateOnboardingPlanTemplate;

public sealed class CreateOnboardingPlanTemplateHandler(
    ISqlRepository<OnboardingPlanTemplate> templateRepository,
    IUnitOfWork unitOfWork,
    OnboardingMapper mapper)
    : ICommandHandler<CreateOnboardingPlanTemplateCommand, OneOf<OnboardingPlanTemplateResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<OnboardingPlanTemplateResponse, ErrorDetailResponse>> Handle(
        CreateOnboardingPlanTemplateCommand request, CancellationToken cancellationToken)
    {
        var id = new OnboardingPlanTemplateId(IdGenerator.NextGuid());
        var template = OnboardingPlanTemplate.Create(id, request.Name, request.Description, request.CreatedBy);

        foreach (var dto in request.TaskTemplates.OrderBy(t => t.SortOrder))
        {
            var taskId = new OnboardingTaskTemplateId(IdGenerator.NextGuid());
            var task = OnboardingTaskTemplate.Create(
                taskId, dto.Title, dto.Description, dto.AssigneeRoleCode,
                dto.OffsetDays, dto.SortOrder, dto.IsRequired);
            template.AddTaskTemplate(task);
        }

        var createResult = await templateRepository.CreateOneAsync(template, cancellationToken);
        if (createResult.TryPickT1(out var exception, out _))
            return HrErrorResponses.FromSaveResult(exception, HrBusinessErrorCodes.SaveChangesFailed);

        var saveResult = await unitOfWork.SaveChangesAsync(cancellationToken);
        if (saveResult.IsT1)
            return HrErrorResponses.FromSaveResult(saveResult.AsT1, HrBusinessErrorCodes.SaveChangesFailed);

        return mapper.ToResponse(template);
    }
}
