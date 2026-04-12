using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Contract.MasterData.ModelIds;
using System.Collections.Generic;

namespace Anemoi.Contract.MasterData.Commands.SeedExecutionCommands.ManualInsertData;

public sealed record ManualInsertDataCommand(
    SeedServerId SeedServerId,
    string TableName,
    Dictionary<string, object> Data) : ICommandVoid;
