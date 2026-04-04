using FluentValidation;

namespace Anemoi.Contract.MasterData.Commands.SeedServerCommands.UpdateSeedServer;

public sealed class UpdateSeedServerValidator : AbstractValidator<UpdateSeedServerCommand>
{
    public UpdateSeedServerValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(512);
        RuleFor(x => x.ConnectionString).NotEmpty();
    }
}
