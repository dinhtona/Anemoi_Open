using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.BulkImport.Models;
using Anemoi.Hr.Domain.Employees;
using Microsoft.EntityFrameworkCore;

namespace Anemoi.Hr.Application.BulkImport.EmployeeImport.Validation;

public sealed class DuplicateValidator
{
    private readonly ISqlRepository<Employee> _employeeRepo;

    public DuplicateValidator(ISqlRepository<Employee> employeeRepo)
    {
        _employeeRepo = employeeRepo;
    }

    public IReadOnlyCollection<BulkImportValidationError> ValidateInFile(
        IReadOnlyCollection<EmployeeImportRowDto> rows)
    {
        var errors = new List<BulkImportValidationError>();

        var codeGroups = rows
            .Select((r, i) => (Row: r, Index: i))
            .Where(x => !string.IsNullOrWhiteSpace(x.Row.EmployeeCode))
            .GroupBy(x => x.Row.EmployeeCode)
            .Where(g => g.Count() > 1);

        foreach (var group in codeGroups)
            foreach (var dup in group.Skip(1))
                errors.Add(new BulkImportValidationError(dup.Index, "EmployeeCode",
                    dup.Row.EmployeeCode, "Duplicate EmployeeCode within file", "Error"));

        var emailGroups = rows
            .Select((r, i) => (Row: r, Index: i))
            .Where(x => !string.IsNullOrWhiteSpace(x.Row.WorkEmail))
            .GroupBy(x => x.Row.WorkEmail)
            .Where(g => g.Count() > 1);

        foreach (var group in emailGroups)
            foreach (var dup in group.Skip(1))
                errors.Add(new BulkImportValidationError(dup.Index, "WorkEmail",
                    dup.Row.WorkEmail, "Duplicate WorkEmail within file", "Error"));

        return errors;
    }

    public async Task<IReadOnlyCollection<BulkImportValidationError>> ValidateAgainstDatabaseAsync(
        IReadOnlyCollection<EmployeeImportRowDto> rows,
        CancellationToken ct)
    {
        var errors = new List<BulkImportValidationError>();

        var rowList = rows.ToList();
        var codes = rowList.Where(r => !string.IsNullOrWhiteSpace(r.EmployeeCode))
            .Select(r => r.EmployeeCode).Distinct().ToList();
        var emails = rowList.Where(r => !string.IsNullOrWhiteSpace(r.WorkEmail))
            .Select(r => r.WorkEmail).Distinct().ToList();

        if (codes.Count > 0)
        {
            var existingCodes = await _employeeRepo.GetQueryable()
                .Where(e => codes.Contains(e.EmployeeCode))
                .Select(e => e.EmployeeCode)
                .ToListAsync(ct);
            var codeSet = existingCodes.ToHashSet();

            for (var i = 0; i < rowList.Count; i++)
            {
                if (codeSet.Contains(rowList[i].EmployeeCode))
                    errors.Add(new BulkImportValidationError(i, "EmployeeCode",
                        rowList[i].EmployeeCode, "EmployeeCode already exists in system", "Error"));
            }
        }

        if (emails.Count > 0)
        {
            var existingEmails = await _employeeRepo.GetQueryable()
                .Where(e => emails.Contains(e.WorkEmail))
                .Select(e => e.WorkEmail)
                .ToListAsync(ct);
            var emailSet = existingEmails.ToHashSet();

            for (var i = 0; i < rowList.Count; i++)
            {
                if (emailSet.Contains(rowList[i].WorkEmail))
                    errors.Add(new BulkImportValidationError(i, "WorkEmail",
                        rowList[i].WorkEmail, "WorkEmail already exists in system", "Error"));
            }
        }

        return errors;
    }
}
