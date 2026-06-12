using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Hr.Application.Responses;

namespace Anemoi.Hr.Application.Cqrs.Commands.TaxationCommands.DeleteTaxBracket;

public sealed record DeleteTaxBracketCommand(
    string Id) : ICommandResult<SuccessResponse>;
