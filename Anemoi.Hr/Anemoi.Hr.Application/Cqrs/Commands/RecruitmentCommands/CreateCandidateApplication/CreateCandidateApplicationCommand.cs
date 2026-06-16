using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;
using System.Text.Json.Serialization;

namespace Anemoi.Hr.Application.Cqrs.Commands.RecruitmentCommands.CreateCandidateApplication;

public sealed record CreateCandidateApplicationCommand(
    CandidateId CandidateId,
    JobPostingId JobPostingId) : ICommandResult<CandidateApplicationResponse>
{
    [JsonIgnore]
    public string CreatedBy { get; set; }
}
