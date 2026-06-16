using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;
using System;
using System.Text.Json.Serialization;

namespace Anemoi.Hr.Application.Cqrs.Commands.RecruitmentCommands.CreateRequisition;

public sealed record CreateRequisitionCommand(
    string RequisitionCode,
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
    public string CreatedBy { get; set; }
}
