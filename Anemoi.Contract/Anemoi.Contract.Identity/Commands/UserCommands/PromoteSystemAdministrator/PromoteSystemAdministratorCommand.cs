using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Contract.Identity.ModelIds;

namespace Anemoi.Contract.Identity.Commands.UserCommands.PromoteSystemAdministrator;

public sealed record PromoteSystemAdministratorCommand(UserId UserId) : ICommandVoid;
