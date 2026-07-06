using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Application.Cqrs.Commands.DepartmentCommands.CreateDepartment;

public sealed record CreateDepartmentCommand(
    string Code,
    string Name,
    string DepartmentTypeCode,
    DepartmentId? ParentDepartmentId,
    EmployeeId? ManagerEmployeeId) : ICommandResult<DepartmentResponse>;
