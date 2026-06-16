using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;
using System.Text.Json.Serialization;

namespace Anemoi.Hr.Application.Cqrs.Commands.RecruitmentCommands.SubmitInterviewFeedback;

public sealed record SubmitInterviewFeedbackCommand(
    InterviewScheduleId InterviewScheduleId,
    int Rating,
    string Strengths,
    string Concerns,
    string Recommendation) : ICommandResult<InterviewFeedbackResponse>
{
    [JsonIgnore]
    public EmployeeId InterviewerEmployeeId { get; set; }
}
