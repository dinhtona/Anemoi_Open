using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Cqrs.Commands.EmployeeImport;
using OneOf;

namespace Anemoi.Hr.Application.BulkImport.Abstractions;

public interface IImportOrchestrator
{
    Task<OneOf<EmployeeImportResultResponse, ErrorDetailResponse>> ExecuteImportAsync(
        Domain.BulkImport.BulkImportJob job,
        string actorUserId,
        string? actorName,
        CancellationToken ct);
}
