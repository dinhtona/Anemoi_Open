using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;
using System.Text.Json.Serialization;

namespace Anemoi.Hr.Application.Cqrs.Commands.RecruitmentCommands.MoveCandidateApplicationToInterview;

public sealed record MoveCandidateApplicationToInterviewCommand(CandidateApplicationId Id, string Note,

    [property: JsonIgnore] string? ChangedBy = null)
    : ICommandResult<CandidateApplicationResponse>
{
}
