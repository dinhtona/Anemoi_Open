using System.Text.Json.Serialization;
using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Application.Cqrs.Commands.EmployeeAssetCommands.ReturnEmployeeAsset;

public sealed record ReturnEmployeeAssetCommand(
    EmployeeAssetId Id,
    [property: JsonIgnore] string ReturnedBy = null
) : ICommandVoid;
