using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Application.Cqrs.Commands.CalendarManagementCommands.PublicHoliday.UpdatePublicHoliday;

public sealed record UpdatePublicHolidayCommand(
    PublicHolidayId Id,
    DateOnly HolidayDate,
    string Name,
    string? Description,
    string CountryCode,
    bool IsRecurringAnnual) : ICommandResult<SuccessResponse>;
