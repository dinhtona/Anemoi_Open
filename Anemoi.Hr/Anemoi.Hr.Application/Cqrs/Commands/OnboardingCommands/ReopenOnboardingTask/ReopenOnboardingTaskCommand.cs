using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;
using OneOf;
using Anemoi.BuildingBlock.Application.Responses;
using System.Text.Json.Serialization;

namespace Anemoi.Hr.Application.Cqrs.Commands.OnboardingCommands.ReopenOnboardingTask;

public sealed record ReopenOnboardingTaskCommand(
    OnboardingTaskId TaskId,
    string? Reason) : ICommandResult<OnboardingInstanceResponse>
{
    [JsonIgnore]
    public string? ReopenedBy { get; set; }
}
