using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Application.Cqrs.Commands.CalendarManagementCommands.CompanyHoliday.DeleteCompanyHoliday;

public sealed record DeleteCompanyHolidayCommand(
    CompanyHolidayId Id) : ICommandResult<SuccessResponse>;
