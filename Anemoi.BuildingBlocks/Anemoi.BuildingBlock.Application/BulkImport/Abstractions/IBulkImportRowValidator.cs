#nullable enable
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Anemoi.BuildingBlock.Application.BulkImport.Models;

namespace Anemoi.BuildingBlock.Application.BulkImport.Abstractions;

public interface IBulkImportRowValidator<in TRow>
{
    Task<IReadOnlyCollection<BulkImportValidationError>> ValidateAsync(
        TRow row, int rowIndex, CancellationToken ct);
}
