using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Hr.Application.Responses;
using System.Text.Json.Serialization;

namespace Anemoi.Hr.Application.Cqrs.Commands.OnboardingCommands.CreateOnboardingPlanTemplate;

public sealed record CreateOnboardingPlanTemplateCommand(
    string Name,
    string? Description,
    List<CreateOnboardingTaskTemplateDto> TaskTemplates) : ICommandResult<OnboardingPlanTemplateResponse>
{
    [JsonIgnore]
    public string? CreatedBy { get; set; }
}

public sealed record CreateOnboardingTaskTemplateDto(
    string Title,
    string? Description,
    string? AssigneeRoleCode,
    int OffsetDays,
    int SortOrder,
    bool IsRequired = true);
