using System.Text.Json.Serialization;
using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Application.Cqrs.Commands.EmployeeAssetCommands.AssignEmployeeAsset;

public sealed record AssignEmployeeAssetCommand(
    EmployeeId EmployeeId,
    string AssetType,
    string AssetTag,
    string Name,
    string? Brand,
    string? Model,
    string? SerialNumber,
    string? Notes,
    [property: JsonIgnore] string CreatedBy = null
) : ICommandVoid;
