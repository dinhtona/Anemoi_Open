using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Contract.MasterData.ModelIds;
using Newtonsoft.Json;

namespace Anemoi.Contract.MasterData.Commands.SeedServerCommands.UpdateSeedServer;

public sealed record UpdateSeedServerCommand(
    string Name, 
    string ConnectionString, 
    string Environment,
    string Provider,
    [property: JsonIgnore] SeedServerId Id = default) : ICommandVoid;
