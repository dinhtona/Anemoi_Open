#nullable enable
using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Contract.MasterData.ModelIds;
using Newtonsoft.Json;

namespace Anemoi.Contract.MasterData.Commands.SeedFunctionCommands.UpdateSeedFunction;

public sealed record UpdateSeedFunctionCommand(
    string Name,
    string Description,
    string TablesJson,
    SeedServerId? SeedServerId = default,
    SeedTemplateId? SeedTemplateId = default,
    [property: JsonIgnore] SeedFunctionId Id = default!) : ICommandVoid;
