using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Contract.MasterData.ModelIds;
using Newtonsoft.Json;

namespace Anemoi.Contract.MasterData.Commands.SeedServerCommands.UpdateSeedServer;

public sealed record UpdateSeedServerCommand(
    [property: JsonIgnore] SeedServerId Id, 
    string Name, 
    string ConnectionString, 
    string Environment) : ICommandVoid;
