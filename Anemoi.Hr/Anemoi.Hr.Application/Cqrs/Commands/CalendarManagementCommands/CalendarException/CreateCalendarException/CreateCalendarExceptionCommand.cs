using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Hr.Domain.CalendarManagement;
using Anemoi.Hr.Application.Responses;

namespace Anemoi.Hr.Application.Cqrs.Commands.CalendarManagementCommands.CalendarException.CreateCalendarException;

public sealed record CreateCalendarExceptionCommand(
    DateOnly ExceptionDate,
    string ExceptionType,
    string Name,
    string? Description,
    Guid? RelatedHolidayId) : ICommandResult<CalendarExceptionIdResponse>;
