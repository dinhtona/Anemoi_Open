using Anemoi.BuildingBlock.Application.Cqrs.Commands;

namespace Anemoi.Contract.MasterData.Commands.SeedServerCommands.TestSeedServerConnection;

public sealed record TestSeedServerConnectionCommand(string ConnectionString) : ICommandVoid;
