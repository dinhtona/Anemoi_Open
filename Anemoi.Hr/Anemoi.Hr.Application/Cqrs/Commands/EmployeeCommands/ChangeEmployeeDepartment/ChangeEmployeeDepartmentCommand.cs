using System.Text.Json.Serialization;
using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Application.Cqrs.Commands.EmployeeCommands.ChangeEmployeeDepartment;

public sealed record ChangeEmployeeDepartmentCommand(
    EmployeeId EmployeeId,
    DepartmentId NewDepartmentId,
    [property: JsonIgnore] string CreatedBy = null) : ICommandVoid;
