using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;
using OneOf;
using Anemoi.BuildingBlock.Application.Responses;
using System.Text.Json.Serialization;

namespace Anemoi.Hr.Application.Cqrs.Commands.OnboardingCommands.CompleteOnboardingTask;

public sealed record CompleteOnboardingTaskCommand(
    OnboardingTaskId TaskId,
    string? Notes) : ICommandResult<OnboardingInstanceResponse>
{
    [JsonIgnore]
    public string? CompletedBy { get; set; }
}
