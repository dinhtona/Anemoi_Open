using System.Text.Json.Serialization;
using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Application.Cqrs.Commands.EmployeeCommands.ResumeEmployee;

public sealed record ResumeEmployeeCommand(
    EmployeeId EmployeeId,
    [property: JsonIgnore] string CreatedBy = null) : ICommandVoid;
