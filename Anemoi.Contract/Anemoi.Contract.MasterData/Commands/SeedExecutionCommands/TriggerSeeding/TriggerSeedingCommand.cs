using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Contract.MasterData.ModelIds;
using Anemoi.Contract.MasterData.Responses;

namespace Anemoi.Contract.MasterData.Commands.SeedExecutionCommands.TriggerSeeding;

public sealed record TriggerSeedingCommand(SeedFunctionId SeedFunctionId) : ICommandResult<TriggerSeedingResponse>;
