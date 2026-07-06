using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Contract.Identity.Responses;

namespace Anemoi.Contract.Identity.Commands.RefreshTokenCommands.ExternalLogin;

public sealed record ExternalLoginCommand(string Provider, string Token) : ICommandResult<AuthenticationSuccessResponse>;
