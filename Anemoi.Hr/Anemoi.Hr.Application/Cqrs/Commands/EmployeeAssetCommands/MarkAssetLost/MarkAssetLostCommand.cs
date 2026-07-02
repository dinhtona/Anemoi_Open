using System.Text.Json.Serialization;
using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Application.Cqrs.Commands.EmployeeAssetCommands.MarkAssetLost;

public sealed record MarkAssetLostCommand(
    EmployeeAssetId Id,
    [property: JsonIgnore] string UpdatedBy = null
) : ICommandVoid;
