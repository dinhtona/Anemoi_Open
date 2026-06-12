using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Hr.Application.Responses;

namespace Anemoi.Hr.Application.Cqrs.Commands.CalendarManagementCommands.CompanyHoliday.CreateCompanyHoliday;

public sealed record CreateCompanyHolidayCommand(
    DateOnly HolidayDate,
    string Name,
    string? Description,
    bool IsRecurringAnnual) : ICommandResult<CompanyHolidayIdResponse>;
