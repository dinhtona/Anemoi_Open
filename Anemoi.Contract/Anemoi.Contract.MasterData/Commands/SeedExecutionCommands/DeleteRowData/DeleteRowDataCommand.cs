using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Contract.MasterData.ModelIds;

namespace Anemoi.Contract.MasterData.Commands.SeedExecutionCommands.DeleteRowData;

public sealed record DeleteRowDataCommand(
    SeedServerId SeedServerId,
    string TableName,
    string PrimaryKeyCondition) : ICommandVoid;
