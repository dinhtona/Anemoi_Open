using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;
using OneOf;
using Anemoi.BuildingBlock.Application.Responses;
using System.Text.Json.Serialization;

namespace Anemoi.Hr.Application.Cqrs.Commands.OnboardingCommands.SkipOnboardingTask;

public sealed record SkipOnboardingTaskCommand(
    OnboardingTaskId TaskId) : ICommandResult<OnboardingInstanceResponse>
{
    [JsonIgnore]
    public string SkippedBy { get; set; }
}
