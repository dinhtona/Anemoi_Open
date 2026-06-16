using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;
using System.Text.Json.Serialization;

namespace Anemoi.Hr.Application.Cqrs.Commands.RecruitmentCommands.ArchiveCandidate;

public sealed record ArchiveCandidateCommand(CandidateId Id)
    : ICommandResult<CandidateResponse>
{
    [JsonIgnore]
    public string UpdatedBy { get; set; }
}
