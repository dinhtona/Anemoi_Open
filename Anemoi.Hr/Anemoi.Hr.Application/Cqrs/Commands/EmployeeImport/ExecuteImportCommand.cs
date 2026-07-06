using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.Hr.ModelIds.ModelIds;
using OneOf;

namespace Anemoi.Hr.Application.Cqrs.Commands.EmployeeImport;

public sealed record ExecuteImportCommand(
    BulkImportJobId JobId,
    string? ActorUserId = null,
    string? ActorName = null) : ICommandResult<EmployeeImportResultResponse>;
