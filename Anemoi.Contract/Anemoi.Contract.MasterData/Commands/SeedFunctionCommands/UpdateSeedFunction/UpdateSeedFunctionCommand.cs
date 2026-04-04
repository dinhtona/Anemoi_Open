using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Contract.MasterData.ModelIds;
using Newtonsoft.Json;

namespace Anemoi.Contract.MasterData.Commands.SeedFunctionCommands.UpdateSeedFunction;

public sealed record UpdateSeedFunctionCommand(
    [property: JsonIgnore] SeedFunctionId Id,
    SeedServerId? SeedServerId,
    SeedTemplateId? SeedTemplateId,
    string Name,
    string Description,
    string TablesJson) : ICommandVoid;
