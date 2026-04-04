using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Contract.MasterData.ModelIds;

namespace Anemoi.Contract.MasterData.Commands.SeedFunctionCommands.CreateSeedFunction;

public sealed record CreateSeedFunctionCommand(
    SeedServerId SeedServerId, 
    SeedTemplateId? SeedTemplateId, 
    string Name, 
    string Description, 
    string TablesJson) : ICommandVoid;
