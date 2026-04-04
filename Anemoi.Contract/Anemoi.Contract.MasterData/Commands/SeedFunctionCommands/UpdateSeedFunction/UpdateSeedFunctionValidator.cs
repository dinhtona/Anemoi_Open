using FluentValidation;

namespace Anemoi.Contract.MasterData.Commands.SeedFunctionCommands.UpdateSeedFunction;

public sealed class UpdateSeedFunctionValidator : AbstractValidator<UpdateSeedFunctionCommand>
{
    public UpdateSeedFunctionValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(512);
    }
}
