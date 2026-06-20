using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Application.Cqrs.Commands.LeaveTypeCommands.DeactivateLeaveType;

public sealed record DeactivateLeaveTypeCommand(LeaveTypeId Id) : ICommandResult<SuccessResponse>;
