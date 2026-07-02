using System.Text.Json.Serialization;
using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Application.Cqrs.Commands.EmployeeCommands.CreateEmployee;

public sealed record CreateEmployeeCommand(
    string EmployeeCode,
    string FirstName,
    string LastName,
    string? DisplayName,
    string? AvatarStorageKey,
    string WorkEmail,
    string? PersonalEmail,
    string PhoneNumber,
    DateOnly? DateOfBirth,
    DateOnly JoinDate,
    string EmploymentTypeCode,
    string? GradeCode,
    DepartmentId PrimaryDepartmentId,
    PositionId PrimaryPositionId,
    EmployeeId? DirectManagerEmployeeId,
    [property: JsonIgnore] string CreatedBy = null
) : ICommandResult<CreateEmployeeResponse>;
