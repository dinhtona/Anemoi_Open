using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;
using OneOf;
using Anemoi.BuildingBlock.Application.Responses;
using System.Text.Json.Serialization;

namespace Anemoi.Hr.Application.Cqrs.Commands.OnboardingCommands.CancelOnboarding;

public sealed record CancelOnboardingCommand(
    OnboardingInstanceId InstanceId,

    [property: JsonIgnore] string? CancelledBy = null) : ICommandResult<OnboardingInstanceResponse>
{
}
