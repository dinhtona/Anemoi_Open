using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;
using System.Text.Json.Serialization;

namespace Anemoi.Hr.Application.Cqrs.Commands.RecruitmentCommands.WithdrawCandidateApplication;

public sealed record WithdrawCandidateApplicationCommand(CandidateApplicationId Id, string Note,

    [property: JsonIgnore] string? ChangedBy = null)
    : ICommandResult<CandidateApplicationResponse>
{
}
