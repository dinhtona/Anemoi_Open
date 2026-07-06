using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;
using System.Text.Json.Serialization;

namespace Anemoi.Hr.Application.Cqrs.Commands.RecruitmentCommands.ApproveRequisition;

public sealed record ApproveRequisitionCommand(JobRequisitionId Id,

    [property: JsonIgnore] string? ApprovedBy = null)
    : ICommandResult<JobRequisitionResponse>
{
}
