using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Contract.Identity.ModelIds;

namespace Anemoi.Contract.Identity.Commands.UserCommands.RevokeUserSession;

public sealed record RevokeUserSessionCommand(UserId UserId) : ICommandVoid;
