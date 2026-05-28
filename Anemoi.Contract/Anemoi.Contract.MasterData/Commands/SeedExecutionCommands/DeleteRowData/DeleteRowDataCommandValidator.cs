using FluentValidation;

namespace Anemoi.Contract.MasterData.Commands.SeedExecutionCommands.DeleteRowData;

public sealed class DeleteRowDataCommandValidator : AbstractValidator<DeleteRowDataCommand>
{
    public DeleteRowDataCommandValidator()
    {
        RuleFor(x => x.SeedServerId).NotEmpty();
        RuleFor(x => x.TableName).NotEmpty().MaximumLength(256);
        RuleFor(x => x.PrimaryKeyCondition)
            .NotEmpty()
            .MaximumLength(1024)
            .Matches(@"^[a-zA-Z_][a-zA-Z0-9_]*\s*=\s*(?:'[^']*'|\d+)(?:\s+AND\s+[a-zA-Z_][a-zA-Z0-9_]*\s*=\s*(?:'[^']*'|\d+))*$")
            .WithMessage("Primary key condition must be a valid key-value equality pair (e.g., 'Id = 123' or 'Id = \'value\'') and can only be combined using 'AND'.");
    }
}
