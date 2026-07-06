using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;
using OneOf;
using Anemoi.BuildingBlock.Application.Responses;
using System.Text.Json.Serialization;

namespace Anemoi.Hr.Application.Cqrs.Commands.OnboardingCommands.StartOnboarding;

public sealed record StartOnboardingCommand(
    EmployeeId EmployeeId,
    OnboardingPlanTemplateId TemplateId,
    DateTime StartDate,
    Dictionary<string, string> RoleMappings,

    [property: JsonIgnore] string? CreatedBy = null) : ICommandResult<OnboardingInstanceResponse>
{
}
