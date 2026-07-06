using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;
using System.Text.Json.Serialization;

namespace Anemoi.Hr.Application.Cqrs.Commands.RecruitmentCommands.BlacklistCandidate;

public sealed record BlacklistCandidateCommand(CandidateId Id,

    [property: JsonIgnore] string? UpdatedBy = null)
    : ICommandResult<CandidateResponse>
{
}
