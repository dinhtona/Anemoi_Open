using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Application.Cqrs.Commands.DepartmentCommands.UpdateDepartment;

public sealed record UpdateDepartmentCommand(
    DepartmentId Id,
    string Code,
    string Name,
    string DepartmentTypeCode,
    DepartmentId? ParentDepartmentId,
    EmployeeId? ManagerEmployeeId) : ICommandResult<DepartmentResponse>;
