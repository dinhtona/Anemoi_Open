using System.Text.Json.Serialization;
using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Application.Cqrs.Commands.EmployeeAssetCommands.MarkAssetDamaged;

public sealed record MarkAssetDamagedCommand(
    EmployeeAssetId Id,
    [property: JsonIgnore] string UpdatedBy = null
) : ICommandVoid;
