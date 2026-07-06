using System.Text.Json.Serialization;
using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Application.Cqrs.Commands.EmployeeCommands.TransferEmployee;

public sealed record TransferEmployeeCommand(
    EmployeeId EmployeeId,
    DepartmentId NewDepartmentId,
    DateOnly EffectiveDate,
    string ReasonCode,
    [property: JsonIgnore] string CreatedBy = null) : ICommandResult<EmployeeDepartmentHistoryIdResponse>;
