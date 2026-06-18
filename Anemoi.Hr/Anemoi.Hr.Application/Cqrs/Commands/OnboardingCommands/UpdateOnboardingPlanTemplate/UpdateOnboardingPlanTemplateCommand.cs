using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;
using System.Text.Json.Serialization;

namespace Anemoi.Hr.Application.Cqrs.Commands.OnboardingCommands.UpdateOnboardingPlanTemplate;

public sealed record UpdateOnboardingPlanTemplateCommand(
    string Name,
    string? Description,
    List<UpdateOnboardingTaskTemplateDto> TaskTemplates,

    [property: JsonIgnore] string? UpdatedBy = null) : ICommandResult<OnboardingPlanTemplateResponse>
{
    [JsonIgnore]
    public OnboardingPlanTemplateId Id { get; set; }
}

public sealed record UpdateOnboardingTaskTemplateDto(
    string? Id,
    string Title,
    string? Description,
    string? AssigneeRoleCode,
    int OffsetDays,
    int SortOrder,
    bool IsRequired = true);
