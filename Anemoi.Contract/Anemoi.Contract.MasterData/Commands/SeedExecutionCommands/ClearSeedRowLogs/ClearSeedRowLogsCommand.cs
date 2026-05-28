using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Contract.MasterData.ModelIds;

namespace Anemoi.Contract.MasterData.Commands.SeedExecutionCommands.ClearSeedRowLogs;

public sealed record ClearSeedRowLogsCommand(SeedServerId SeedServerId) : ICommandVoid;
