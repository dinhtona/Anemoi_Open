using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Application.Cqrs.Commands.ShiftManagementCommands.ActivateShiftTemplate;

public sealed record ActivateShiftTemplateCommand(
    ShiftTemplateId Id) : ICommandResult<SuccessResponse>;
