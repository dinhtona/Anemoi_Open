using System.Text.Json.Serialization;
using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Application.Cqrs.Commands.EmployeeAssetCommands.UpdateEmployeeAsset;

public sealed record UpdateEmployeeAssetCommand(
    EmployeeAssetId Id,
    string Name,
    string? Brand,
    string? Model,
    string? SerialNumber,
    string? Notes,
    [property: JsonIgnore] string UpdatedBy = null
) : ICommandVoid;
