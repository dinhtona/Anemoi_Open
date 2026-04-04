using FluentValidation;

namespace Anemoi.Contract.MasterData.Commands.SeedFunctionCommands.CreateSeedFunction;

public sealed class CreateSeedFunctionValidator : AbstractValidator<CreateSeedFunctionCommand>
{
    public CreateSeedFunctionValidator()
    {
        RuleFor(x => x.SeedServerId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(512);
        RuleFor(x => x.TablesJson).NotEmpty();
    }
}
