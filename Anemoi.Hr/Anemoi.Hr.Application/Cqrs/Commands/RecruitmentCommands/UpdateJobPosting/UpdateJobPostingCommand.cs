using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;
using System;
using System.Text.Json.Serialization;

namespace Anemoi.Hr.Application.Cqrs.Commands.RecruitmentCommands.UpdateJobPosting;

public sealed record UpdateJobPostingCommand(
    JobPostingId Id,
    string PostingTitle,
    string PostingDescription,
    DateOnly PublishDate,
    DateOnly ExpiryDate,

    [property: JsonIgnore] string? UpdatedBy = null) : ICommandResult<JobPostingResponse>
{
}
