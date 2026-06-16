using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;
using System.Text.Json.Serialization;

namespace Anemoi.Hr.Application.Cqrs.Commands.RecruitmentCommands.PublishJobPosting;

public sealed record PublishJobPostingCommand(JobPostingId Id)
    : ICommandResult<JobPostingResponse>
{
    [JsonIgnore]
    public string PublishedBy { get; set; }
}
