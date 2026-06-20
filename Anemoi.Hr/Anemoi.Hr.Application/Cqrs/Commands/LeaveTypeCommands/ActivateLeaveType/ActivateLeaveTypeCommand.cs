using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Application.Cqrs.Commands.LeaveTypeCommands.ActivateLeaveType;

public sealed record ActivateLeaveTypeCommand(LeaveTypeId Id) : ICommandResult<SuccessResponse>;
