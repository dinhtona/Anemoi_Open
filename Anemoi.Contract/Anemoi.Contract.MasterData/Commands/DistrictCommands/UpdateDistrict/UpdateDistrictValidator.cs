using FluentValidation;

namespace Anemoi.Contract.MasterData.Commands.DistrictCommands.UpdateDistrict;

public sealed class UpdateDistrictValidator : AbstractValidator<UpdateDistrictCommand>
{
    public UpdateDistrictValidator()
    {
        When(x => x.Name is { }, () => RuleFor(x => x.Name).NotEmpty());
        When(x => x.ProvinceId is { }, () => RuleFor(x => x.ProvinceId)
            .Must(x => x is { } && x.Value != Guid.Empty)
            .WithMessage("VAL_PROVINCE_ID_REQUIRED"));
        When(x => x.Id is { }, () => RuleFor(x => x.Id)
            .Must(x => x is { } && x.Value != Guid.Empty)
            .WithMessage("VAL_DISTRICT_ID_REQUIRED"));
        ;
    }
}