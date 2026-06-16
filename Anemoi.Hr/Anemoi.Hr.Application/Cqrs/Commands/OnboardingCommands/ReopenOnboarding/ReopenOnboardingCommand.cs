using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;
using OneOf;
using Anemoi.BuildingBlock.Application.Responses;
using System.Text.Json.Serialization;

namespace Anemoi.Hr.Application.Cqrs.Commands.OnboardingCommands.ReopenOnboarding;

public sealed record ReopenOnboardingCommand(
    OnboardingInstanceId InstanceId) : ICommandResult<OnboardingInstanceResponse>
{
    [JsonIgnore]
    public string? ReopenedBy { get; set; }
}
