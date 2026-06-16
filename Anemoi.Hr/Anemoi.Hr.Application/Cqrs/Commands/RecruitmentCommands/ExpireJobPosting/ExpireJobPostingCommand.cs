using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;
using System.Text.Json.Serialization;

namespace Anemoi.Hr.Application.Cqrs.Commands.RecruitmentCommands.ExpireJobPosting;

public sealed record ExpireJobPostingCommand(JobPostingId Id)
    : ICommandResult<JobPostingResponse>
{
    [JsonIgnore]
    public string ExpiredBy { get; set; }
}
