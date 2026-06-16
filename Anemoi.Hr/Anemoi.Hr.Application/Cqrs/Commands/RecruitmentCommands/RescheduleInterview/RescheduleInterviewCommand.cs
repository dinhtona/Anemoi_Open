using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;
using System;
using System.Text.Json.Serialization;

namespace Anemoi.Hr.Application.Cqrs.Commands.RecruitmentCommands.RescheduleInterview;

public sealed record RescheduleInterviewCommand(
    InterviewScheduleId Id,
    DateTime ScheduledAt,
    int DurationMinutes) : ICommandResult<InterviewScheduleResponse>
{
    [JsonIgnore]
    public string UpdatedBy { get; set; }
}
