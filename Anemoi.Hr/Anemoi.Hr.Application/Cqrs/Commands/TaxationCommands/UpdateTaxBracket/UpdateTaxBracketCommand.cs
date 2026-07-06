using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Hr.Application.Responses;

namespace Anemoi.Hr.Application.Cqrs.Commands.TaxationCommands.UpdateTaxBracket;

public sealed record UpdateTaxBracketCommand(
    string Id,
    decimal FromAmount,
    decimal? ToAmount,
    decimal Rate,
    decimal? QuickDeductionAmount,
    int SortOrder) : ICommandResult<SuccessResponse>;
