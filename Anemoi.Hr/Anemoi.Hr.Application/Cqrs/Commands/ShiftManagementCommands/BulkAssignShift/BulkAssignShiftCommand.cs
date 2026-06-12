using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Application.Cqrs.Commands.ShiftManagementCommands.BulkAssignShift;

public sealed record BulkAssignShiftCommand(
    List<EmployeeId> EmployeeIds,
    ShiftTemplateId ShiftTemplateId,
    DateOnly WorkDate,
    string AssignedBy) : ICommandResult<SuccessResponse>;
