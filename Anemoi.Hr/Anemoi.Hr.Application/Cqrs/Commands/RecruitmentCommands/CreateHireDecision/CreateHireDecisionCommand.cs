using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;
using System.Text.Json.Serialization;

namespace Anemoi.Hr.Application.Cqrs.Commands.RecruitmentCommands.CreateHireDecision;

public sealed record CreateHireDecisionCommand(
    CandidateApplicationId CandidateApplicationId,
    string Notes,

    [property: JsonIgnore] string? DecidedBy = null) : ICommandResult<HiringDecisionResponse>
{
}
