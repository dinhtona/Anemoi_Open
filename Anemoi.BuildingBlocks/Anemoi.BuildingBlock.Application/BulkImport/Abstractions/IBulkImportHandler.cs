#nullable enable
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Anemoi.BuildingBlock.Application.BulkImport.Models;
using Anemoi.BuildingBlock.Application.Responses;
using OneOf;

namespace Anemoi.BuildingBlock.Application.BulkImport.Abstractions;

public interface IBulkImportHandler<TRow>
{
    Task<OneOf<IReadOnlyCollection<BulkImportRowResult>, ErrorDetailResponse>> HandleAsync(
        IReadOnlyCollection<TRow> validRows,
        string actorUserId,
        CancellationToken ct);
}
