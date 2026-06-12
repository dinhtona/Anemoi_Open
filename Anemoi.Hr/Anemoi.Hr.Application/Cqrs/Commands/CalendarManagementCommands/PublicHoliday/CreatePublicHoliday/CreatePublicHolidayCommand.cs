using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Hr.Application.Responses;

namespace Anemoi.Hr.Application.Cqrs.Commands.CalendarManagementCommands.PublicHoliday.CreatePublicHoliday;

public sealed record CreatePublicHolidayCommand(
    DateOnly HolidayDate,
    string Name,
    string? Description,
    string CountryCode,
    bool IsRecurringAnnual) : ICommandResult<PublicHolidayIdResponse>;
