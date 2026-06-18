using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;
using OneOf;
using Anemoi.BuildingBlock.Application.Responses;
using System.Text.Json.Serialization;

namespace Anemoi.Hr.Application.Cqrs.Commands.OnboardingCommands.ReassignOnboardingTask;

public sealed record ReassignOnboardingTaskCommand(
    OnboardingTaskId TaskId,
    string NewUserId,
    string? NewUserDisplayName,

    [property: JsonIgnore] string? ReassignedBy = null) : ICommandResult<OnboardingInstanceResponse>
{
}
