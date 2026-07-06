using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;
using System.Text.Json.Serialization;

namespace Anemoi.Hr.Application.Cqrs.Commands.RecruitmentCommands.CloseRequisition;

public sealed record CloseRequisitionCommand(JobRequisitionId Id,

    [property: JsonIgnore] string? ClosedBy = null)
    : ICommandResult<JobRequisitionResponse>
{
}
