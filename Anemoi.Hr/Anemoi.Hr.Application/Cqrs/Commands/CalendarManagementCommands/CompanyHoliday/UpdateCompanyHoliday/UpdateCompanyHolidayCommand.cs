using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Application.Cqrs.Commands.CalendarManagementCommands.CompanyHoliday.UpdateCompanyHoliday;

public sealed record UpdateCompanyHolidayCommand(
    CompanyHolidayId Id,
    DateOnly HolidayDate,
    string Name,
    string? Description,
    bool IsRecurringAnnual) : ICommandResult<SuccessResponse>;
