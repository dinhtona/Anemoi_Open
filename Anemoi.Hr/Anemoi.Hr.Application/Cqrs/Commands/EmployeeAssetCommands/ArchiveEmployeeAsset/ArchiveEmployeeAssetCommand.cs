using System.Text.Json.Serialization;
using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Application.Cqrs.Commands.EmployeeAssetCommands.ArchiveEmployeeAsset;

public sealed record ArchiveEmployeeAssetCommand(
    EmployeeAssetId Id,
    [property: JsonIgnore] string ArchivedBy = null
) : ICommandVoid;
