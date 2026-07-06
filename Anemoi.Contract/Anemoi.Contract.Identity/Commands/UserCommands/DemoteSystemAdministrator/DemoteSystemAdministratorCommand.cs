using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Contract.Identity.ModelIds;

namespace Anemoi.Contract.Identity.Commands.UserCommands.DemoteSystemAdministrator;

public sealed record DemoteSystemAdministratorCommand(UserId UserId) : ICommandVoid;
