using Anemoi.BuildingBlock.Application.BulkImport.Models;

namespace Anemoi.Hr.Application.BulkImport.EmployeeImport.Validation;

public sealed class BusinessValidator
{
    private static readonly HashSet<string> ValidEmploymentTypes =
        ["FullTime", "PartTime", "Contract", "Intern", "Probation"];
    private static readonly HashSet<string> ValidStatuses =
        ["Active", "Onboarding", "PendingOnboarding", "Suspended", "Resigned", "Terminated", "Archived", "Draft"];

    public Task<IReadOnlyCollection<BulkImportValidationError>> ValidateAsync(
        EmployeeImportRowDto row, int rowIndex, CancellationToken ct)
    {
        var errors = new List<BulkImportValidationError>();

        if (!string.IsNullOrWhiteSpace(row.EmploymentType) &&
            !ValidEmploymentTypes.Contains(row.EmploymentType))
            errors.Add(new BulkImportValidationError(rowIndex, "EmploymentType",
                row.EmploymentType!, $"Must be: {string.Join(", ", ValidEmploymentTypes)}", "Error"));

        if (!string.IsNullOrWhiteSpace(row.StatusValue) &&
            !ValidStatuses.Contains(row.StatusValue))
            errors.Add(new BulkImportValidationError(rowIndex, "Status",
                row.StatusValue!, $"Must be: {string.Join(", ", ValidStatuses)}", "Error"));

        return Task.FromResult<IReadOnlyCollection<BulkImportValidationError>>(errors);
    }
}
