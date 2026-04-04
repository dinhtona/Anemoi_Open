using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Contract.MasterData.ModelIds;
using Newtonsoft.Json;

namespace Anemoi.Contract.MasterData.Commands.SeedTemplateCommands.UpdateSeedTemplate;

public sealed record UpdateSeedTemplateCommand(
    string Name,
    string Description,
    string ConfigJson,
    [property: JsonIgnore] SeedTemplateId Id = default) : ICommandVoid;
