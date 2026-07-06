using Anemoi.BuildingBlock.Application.BulkImport.Models;

namespace Anemoi.Hr.Application.BulkImport.EmployeeImport.Validation;

public sealed class SyntaxValidator
{
    public Task<IReadOnlyCollection<BulkImportValidationError>> ValidateAsync(
        EmployeeImportRowDto row, int rowIndex, CancellationToken ct)
    {
        var errors = new List<BulkImportValidationError>();

        if (string.IsNullOrWhiteSpace(row.EmployeeCode))
            errors.Add(Err(rowIndex, "EmployeeCode", row.EmployeeCode, "EmployeeCode is required"));
        if (string.IsNullOrWhiteSpace(row.FirstName))
            errors.Add(Err(rowIndex, "FirstName", row.FirstName, "FirstName is required"));
        if (string.IsNullOrWhiteSpace(row.LastName))
            errors.Add(Err(rowIndex, "LastName", row.LastName, "LastName is required"));
        if (string.IsNullOrWhiteSpace(row.WorkEmail))
            errors.Add(Err(rowIndex, "WorkEmail", row.WorkEmail, "WorkEmail is required"));
        if (string.IsNullOrWhiteSpace(row.DepartmentName))
            errors.Add(Err(rowIndex, "Department", row.DepartmentName ?? "", "Department is required"));
        if (string.IsNullOrWhiteSpace(row.PositionName))
            errors.Add(Err(rowIndex, "Position", row.PositionName ?? "", "Position is required"));

        if (!string.IsNullOrWhiteSpace(row.WorkEmail) && !row.WorkEmail.Contains('@'))
            errors.Add(Err(rowIndex, "WorkEmail", row.WorkEmail, "WorkEmail must contain @"));

        if (!string.IsNullOrWhiteSpace(row.HireDateStr) && !DateOnly.TryParse(row.HireDateStr, out _))
            errors.Add(Err(rowIndex, "HireDate", row.HireDateStr!, "HireDate must be yyyy-MM-dd"));

        if (!string.IsNullOrWhiteSpace(row.DateOfBirthStr) && !DateOnly.TryParse(row.DateOfBirthStr, out _))
            errors.Add(Err(rowIndex, "DOB", row.DateOfBirthStr!, "DOB must be yyyy-MM-dd"));

        return Task.FromResult<IReadOnlyCollection<BulkImportValidationError>>(errors);
    }

    private static BulkImportValidationError Err(int row, string col, string val, string msg) =>
        new(row, col, val, msg, "Error");
}
