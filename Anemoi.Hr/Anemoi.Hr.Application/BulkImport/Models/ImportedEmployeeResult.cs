using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Application.BulkImport.Models;

public sealed record ImportedEmployeeResult(
    EmployeeId EmployeeId,
    int RowIndex);
