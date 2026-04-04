using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Contract.MasterData.ModelIds;
using Newtonsoft.Json;

namespace Anemoi.Contract.MasterData.Commands.SeedTemplateCommands.UpdateSeedTemplate;

public sealed record UpdateSeedTemplateCommand(
    [property: JsonIgnore] SeedTemplateId Id,
    string Name,
    string Description,
    string ConfigJson) : ICommandVoid;
