using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;
using System.Text.Json.Serialization;

namespace Anemoi.Hr.Application.Cqrs.Commands.RecruitmentCommands.RejectRequisition;

public sealed record RejectRequisitionCommand(JobRequisitionId Id, string Reason,

    [property: JsonIgnore] string? RejectedBy = null)
    : ICommandResult<JobRequisitionResponse>
{
}
