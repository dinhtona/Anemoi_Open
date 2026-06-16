using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;
using System;
using System.Text.Json.Serialization;

namespace Anemoi.Hr.Application.Cqrs.Commands.RecruitmentCommands.CreateInterviewSchedule;

public sealed record CreateInterviewScheduleCommand(
    CandidateApplicationId CandidateApplicationId,
    string InterviewType,
    DateTime ScheduledAt,
    int DurationMinutes,
    EmployeeId InterviewerEmployeeId,
    string Notes) : ICommandResult<InterviewScheduleResponse>
{
    [JsonIgnore]
    public string CreatedBy { get; set; }
}
