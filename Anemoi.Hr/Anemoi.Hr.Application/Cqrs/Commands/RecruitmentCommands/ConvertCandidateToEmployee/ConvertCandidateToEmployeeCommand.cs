using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;
using System;
using System.Text.Json.Serialization;

namespace Anemoi.Hr.Application.Cqrs.Commands.RecruitmentCommands.ConvertCandidateToEmployee;

public sealed record ConvertCandidateToEmployeeCommand(
    CandidateId CandidateId,
    string EmployeeCode,
    string FirstName,
    string LastName,
    string? DisplayName,
    string WorkEmail,
    DepartmentId DepartmentId,
    PositionId PositionId,
    DateOnly JoinDate,
    string EmploymentTypeCode,
    string Notes,

    [property: JsonIgnore] string? ConvertedBy = null) : ICommandResult<CandidateConversionResponse>
{
}
