using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Hr.Application.Configurations;
using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Commands.CalendarManagementCommands.CompanyHoliday.CreateCompanyHoliday;

public sealed class CreateCompanyHolidayValidator : AbstractValidator<CreateCompanyHolidayCommand>
{
    public CreateCompanyHolidayValidator()
    {
        RuleFor(x => x.HolidayDate)
            .NotEmpty()
            .WithErrorCode(HrBusinessErrorCodes.CompanyHolidayDateRequired);

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200)
            .WithErrorCode(HrBusinessErrorCodes.CompanyHolidayNameRequired);
    }
}
