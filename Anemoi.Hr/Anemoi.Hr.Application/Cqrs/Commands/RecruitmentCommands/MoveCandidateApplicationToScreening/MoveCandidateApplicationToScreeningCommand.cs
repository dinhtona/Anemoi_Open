using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;
using System.Text.Json.Serialization;

namespace Anemoi.Hr.Application.Cqrs.Commands.RecruitmentCommands.MoveCandidateApplicationToScreening;

public sealed record MoveCandidateApplicationToScreeningCommand(CandidateApplicationId Id, string Note)
    : ICommandResult<CandidateApplicationResponse>
{
    [JsonIgnore]
    public string ChangedBy { get; set; }
}
