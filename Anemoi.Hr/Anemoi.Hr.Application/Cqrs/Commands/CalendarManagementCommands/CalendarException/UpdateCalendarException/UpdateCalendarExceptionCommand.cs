using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Domain.CalendarManagement;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Application.Cqrs.Commands.CalendarManagementCommands.CalendarException.UpdateCalendarException;

public sealed record UpdateCalendarExceptionCommand(
    CalendarExceptionId Id,
    DateOnly ExceptionDate,
    string ExceptionType,
    string Name,
    string? Description,
    Guid? RelatedHolidayId) : ICommandResult<SuccessResponse>;
