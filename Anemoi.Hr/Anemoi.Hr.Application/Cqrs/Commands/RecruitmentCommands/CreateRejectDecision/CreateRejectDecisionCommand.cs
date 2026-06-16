using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;
using System.Text.Json.Serialization;

namespace Anemoi.Hr.Application.Cqrs.Commands.RecruitmentCommands.CreateRejectDecision;

public sealed record CreateRejectDecisionCommand(
    CandidateApplicationId CandidateApplicationId,
    string Notes) : ICommandResult<HiringDecisionResponse>
{
    [JsonIgnore]
    public string DecidedBy { get; set; }
}
