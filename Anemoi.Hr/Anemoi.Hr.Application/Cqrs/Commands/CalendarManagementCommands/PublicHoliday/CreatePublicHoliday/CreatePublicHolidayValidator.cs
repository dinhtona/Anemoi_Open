using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Hr.Application.Configurations;
using FluentValidation;

namespace Anemoi.Hr.Application.Cqrs.Commands.CalendarManagementCommands.PublicHoliday.CreatePublicHoliday;

public sealed class CreatePublicHolidayValidator : AbstractValidator<CreatePublicHolidayCommand>
{
    public CreatePublicHolidayValidator()
    {
        RuleFor(x => x.HolidayDate)
            .NotEmpty()
            .WithErrorCode(HrBusinessErrorCodes.PublicHolidayDateRequired);

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200)
            .WithErrorCode(HrBusinessErrorCodes.PublicHolidayNameRequired);

        RuleFor(x => x.CountryCode)
            .NotEmpty()
            .MaximumLength(10)
            .WithErrorCode(HrBusinessErrorCodes.PublicHolidayCountryCodeRequired);
    }
}
