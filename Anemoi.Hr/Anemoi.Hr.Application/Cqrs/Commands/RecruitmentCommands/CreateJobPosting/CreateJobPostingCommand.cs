using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;
using System;
using System.Text.Json.Serialization;

namespace Anemoi.Hr.Application.Cqrs.Commands.RecruitmentCommands.CreateJobPosting;

public sealed record CreateJobPostingCommand(
    JobRequisitionId JobRequisitionId,
    string PostingTitle,
    string PostingDescription,
    DateOnly PublishDate,
    DateOnly ExpiryDate,

    [property: JsonIgnore] string? CreatedBy = null) : ICommandResult<JobPostingResponse>
{
}
