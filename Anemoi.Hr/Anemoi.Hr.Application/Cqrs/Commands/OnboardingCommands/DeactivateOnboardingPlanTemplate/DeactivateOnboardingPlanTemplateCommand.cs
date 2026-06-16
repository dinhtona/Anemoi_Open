using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;
using System.Text.Json.Serialization;

namespace Anemoi.Hr.Application.Cqrs.Commands.OnboardingCommands.DeactivateOnboardingPlanTemplate;

public sealed record DeactivateOnboardingPlanTemplateCommand(
    OnboardingPlanTemplateId Id) : ICommandResult<OnboardingPlanTemplateResponse>
{
    [JsonIgnore]
    public string? UpdatedBy { get; set; }
}
