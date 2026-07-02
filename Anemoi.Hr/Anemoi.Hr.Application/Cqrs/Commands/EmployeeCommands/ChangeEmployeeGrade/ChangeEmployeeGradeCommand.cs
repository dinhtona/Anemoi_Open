using System.Text.Json.Serialization;
using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Application.Cqrs.Commands.EmployeeCommands.ChangeEmployeeGrade;

public sealed record ChangeEmployeeGradeCommand(
    EmployeeId EmployeeId,
    string NewGradeCode,
    [property: JsonIgnore] string CreatedBy = null) : ICommandVoid;
