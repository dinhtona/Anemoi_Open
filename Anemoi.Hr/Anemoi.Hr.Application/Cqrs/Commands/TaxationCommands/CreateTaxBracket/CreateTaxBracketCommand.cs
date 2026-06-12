using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Hr.Application.Responses;

namespace Anemoi.Hr.Application.Cqrs.Commands.TaxationCommands.CreateTaxBracket;

public sealed record CreateTaxBracketCommand(
    string TaxRuleSetId,
    decimal FromAmount,
    decimal? ToAmount,
    decimal Rate,
    decimal? QuickDeductionAmount,
    int SortOrder) : ICommandResult<CreateTaxBracketResponse>;
