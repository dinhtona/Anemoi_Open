using FluentValidation;

namespace Anemoi.Contract.MasterData.Commands.SeedTemplateCommands.CreateSeedTemplate;

public sealed class CreateSeedTemplateValidator : AbstractValidator<CreateSeedTemplateCommand>
{
    public CreateSeedTemplateValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(512);
        RuleFor(x => x.ConfigJson).NotEmpty();
    }
}
