using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;
using System.Text.Json.Serialization;

namespace Anemoi.Hr.Application.Cqrs.Commands.RecruitmentCommands.MarkInterviewFailed;

public sealed record MarkInterviewFailedCommand(InterviewScheduleId Id)
    : ICommandResult<InterviewScheduleResponse>
{
    [JsonIgnore]
    public string UpdatedBy { get; set; }
}
