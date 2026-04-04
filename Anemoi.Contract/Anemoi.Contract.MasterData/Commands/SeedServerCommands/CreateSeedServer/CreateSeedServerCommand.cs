using Anemoi.BuildingBlock.Application.Cqrs.Commands;

namespace Anemoi.Contract.MasterData.Commands.SeedServerCommands.CreateSeedServer;

public sealed record CreateSeedServerCommand(string Name, string ConnectionString, string Environment) : ICommandVoid;
