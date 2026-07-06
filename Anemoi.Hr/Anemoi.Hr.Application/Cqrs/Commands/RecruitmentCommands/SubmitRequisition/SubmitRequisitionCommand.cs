using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;
using System.Text.Json.Serialization;

namespace Anemoi.Hr.Application.Cqrs.Commands.RecruitmentCommands.SubmitRequisition;

public sealed record SubmitRequisitionCommand(JobRequisitionId Id,

    [property: JsonIgnore] string? SubmittedBy = null)
    : ICommandResult<JobRequisitionResponse>
{
}
