using Anemoi.BuildingBlock.Application.Cqrs.Commands;

namespace Anemoi.Contract.MasterData.Commands.SeedTemplateCommands.CreateSeedTemplate;

public sealed record CreateSeedTemplateCommand(
    string Name, 
    string Description, 
    string ConfigJson) : ICommandVoid;
