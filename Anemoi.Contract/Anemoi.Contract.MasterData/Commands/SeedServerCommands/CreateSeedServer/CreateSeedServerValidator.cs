using FluentValidation;

namespace Anemoi.Contract.MasterData.Commands.SeedServerCommands.CreateSeedServer;

public sealed class CreateSeedServerValidator : AbstractValidator<CreateSeedServerCommand>
{
    public CreateSeedServerValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(512);
        RuleFor(x => x.ConnectionString).NotEmpty();
    }
}
