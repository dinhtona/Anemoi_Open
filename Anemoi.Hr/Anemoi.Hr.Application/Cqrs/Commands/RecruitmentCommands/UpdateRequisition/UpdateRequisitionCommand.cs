using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;
using System;
using System.Text.Json.Serialization;

namespace Anemoi.Hr.Application.Cqrs.Commands.RecruitmentCommands.UpdateRequisition;

public sealed record UpdateRequisitionCommand(
    JobRequisitionId Id,
    string Title,
    DepartmentId DepartmentId,
    PositionId PositionId,
    int Headcount,
    string EmploymentType,
    DateOnly OpenDate,
    DateOnly TargetHireDate,
    string Description) : ICommandResult<JobRequisitionResponse>
{
    [JsonIgnore]
    public string UpdatedBy { get; set; }
}
