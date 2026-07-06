using FluentValidation;

namespace Anemoi.Contract.MasterData.Commands.SeedTemplateCommands.UpdateSeedTemplate;

public sealed class UpdateSeedTemplateValidator : AbstractValidator<UpdateSeedTemplateCommand>
{
    public UpdateSeedTemplateValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(512);
    }
}
