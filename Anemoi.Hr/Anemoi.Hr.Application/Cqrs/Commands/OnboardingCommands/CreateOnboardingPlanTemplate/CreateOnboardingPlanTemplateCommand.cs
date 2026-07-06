using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Hr.Application.Responses;
using System.Text.Json.Serialization;

namespace Anemoi.Hr.Application.Cqrs.Commands.OnboardingCommands.CreateOnboardingPlanTemplate;

public sealed record CreateOnboardingPlanTemplateCommand(
    string Name,
    string? Description,
    List<CreateOnboardingTaskTemplateDto> TaskTemplates,

    [property: JsonIgnore] string? CreatedBy = null) : ICommandResult<OnboardingPlanTemplateResponse>
{
}

public sealed record CreateOnboardingTaskTemplateDto(
    string Title,
    string? Description,
    string? AssigneeRoleCode,
    int OffsetDays,
    int SortOrder,
    bool IsRequired = true);
