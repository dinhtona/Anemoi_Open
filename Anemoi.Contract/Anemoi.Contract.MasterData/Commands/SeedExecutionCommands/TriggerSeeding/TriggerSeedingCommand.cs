using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Contract.MasterData.ModelIds;

namespace Anemoi.Contract.MasterData.Commands.SeedExecutionCommands.TriggerSeeding;

public sealed record TriggerSeedingCommand(SeedFunctionId SeedFunctionId) : ICommandVoid;
