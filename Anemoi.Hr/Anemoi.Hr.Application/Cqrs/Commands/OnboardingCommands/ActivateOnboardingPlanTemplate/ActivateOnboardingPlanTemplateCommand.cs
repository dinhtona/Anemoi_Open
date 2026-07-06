using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;
using System.Text.Json.Serialization;

namespace Anemoi.Hr.Application.Cqrs.Commands.OnboardingCommands.ActivateOnboardingPlanTemplate;

public sealed record ActivateOnboardingPlanTemplateCommand(
    OnboardingPlanTemplateId Id,

    [property: JsonIgnore] string? UpdatedBy = null) : ICommandResult<OnboardingPlanTemplateResponse>
{
}
