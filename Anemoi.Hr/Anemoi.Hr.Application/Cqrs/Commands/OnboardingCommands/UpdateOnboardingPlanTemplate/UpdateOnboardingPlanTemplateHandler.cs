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

namespace Anemoi.Hr.Application.Cqrs.Commands.OnboardingCommands.UpdateOnboardingPlanTemplate;

public sealed class UpdateOnboardingPlanTemplateHandler(
    ISqlRepository<OnboardingPlanTemplate> templateRepository,
    IUnitOfWork unitOfWork,
    OnboardingMapper mapper)
    : ICommandHandler<UpdateOnboardingPlanTemplateCommand, OneOf<OnboardingPlanTemplateResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<OnboardingPlanTemplateResponse, ErrorDetailResponse>> Handle(
        UpdateOnboardingPlanTemplateCommand request, CancellationToken cancellationToken)
    {
        var template = await templateRepository.GetFirstByConditionAsync(
            x => x.Id == request.Id, null, cancellationToken);
        if (template is null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.HrOnboardingTemplateNotFound);

        var updatedTasks = request.TaskTemplates
            .OrderBy(t => t.SortOrder)
            .Select(dto =>
            {
                var taskId = !string.IsNullOrEmpty(dto.Id)
                    ? new OnboardingTaskTemplateId(Guid.Parse(dto.Id))
                    : new OnboardingTaskTemplateId(IdGenerator.NextGuid());
                return OnboardingTaskTemplate.Create(
                    taskId, dto.Title, dto.Description, dto.AssigneeRoleCode,
                    dto.OffsetDays, dto.SortOrder, dto.IsRequired);
            }).ToList();

        template.UpdateDetails(request.Name, request.Description, request.UpdatedBy, updatedTasks);

        var saveResult = await unitOfWork.SaveChangesAsync(cancellationToken);
        if (saveResult.IsT1)
            return HrErrorResponses.FromSaveResult(saveResult.AsT1, HrBusinessErrorCodes.SaveChangesFailed);

        return mapper.ToResponse(template);
    }
}
