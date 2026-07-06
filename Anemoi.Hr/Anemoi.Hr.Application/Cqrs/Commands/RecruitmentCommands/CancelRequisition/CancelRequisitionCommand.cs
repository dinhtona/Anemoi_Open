using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;
using System.Text.Json.Serialization;

namespace Anemoi.Hr.Application.Cqrs.Commands.RecruitmentCommands.CancelRequisition;

public sealed record CancelRequisitionCommand(JobRequisitionId Id, string Reason,

    [property: JsonIgnore] string? CancelledBy = null)
    : ICommandResult<JobRequisitionResponse>
{
}
