using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.BulkImport.Models;
using Anemoi.Hr.Domain.Departments;
using Anemoi.Hr.Domain.Employees;
using Anemoi.Hr.Domain.Positions;
using Microsoft.EntityFrameworkCore;

namespace Anemoi.Hr.Application.BulkImport.EmployeeImport.Validation;

public sealed class ReferenceValidator
{
    private readonly ISqlRepository<Department> _deptRepo;
    private readonly ISqlRepository<Position> _posRepo;
    private readonly ISqlRepository<Employee> _empRepo;

    public ReferenceValidator(
        ISqlRepository<Department> deptRepo,
        ISqlRepository<Position> posRepo,
        ISqlRepository<Employee> empRepo)
    {
        _deptRepo = deptRepo;
        _posRepo = posRepo;
        _empRepo = empRepo;
    }

    public async Task<(IReadOnlyCollection<BulkImportValidationError> Errors,
        Dictionary<string, Department> Departments,
        Dictionary<string, Position> Positions,
        Dictionary<string, Employee> Managers)>
        ValidateAndResolveAsync(IReadOnlyCollection<EmployeeImportRowDto> rows, CancellationToken ct)
    {
        var errors = new List<BulkImportValidationError>();

        var deptNames = rows.Where(r => r.DepartmentName != null)
            .Select(r => r.DepartmentName!).Distinct().ToList();
        var departments = await _deptRepo.GetQueryable()
            .Where(d => deptNames.Contains(d.Name))
            .ToDictionaryAsync(d => d.Name, ct);

        var posNames = rows.Where(r => r.PositionName != null)
            .Select(r => r.PositionName!).Distinct().ToList();
        var positions = await _posRepo.GetQueryable()
            .Where(p => posNames.Contains(p.Name))
            .ToDictionaryAsync(p => p.Name, ct);

        var mgrCodes = rows.Where(r => r.ManagerCode != null)
            .Select(r => r.ManagerCode!).Distinct().ToList();
        var managers = await _empRepo.GetQueryable()
            .Where(m => mgrCodes.Contains(m.EmployeeCode))
            .ToDictionaryAsync(m => m.EmployeeCode, ct);

        var rowList = rows.ToList();
        for (var i = 0; i < rowList.Count; i++)
        {
            var row = rowList[i];

            if (row.DepartmentName != null && !departments.ContainsKey(row.DepartmentName))
                errors.Add(new BulkImportValidationError(i, "Department", row.DepartmentName,
                    $"Department '{row.DepartmentName}' not found", "Error"));

            if (row.PositionName != null && !positions.ContainsKey(row.PositionName))
                errors.Add(new BulkImportValidationError(i, "Position", row.PositionName,
                    $"Position '{row.PositionName}' not found", "Error"));

            if (row.ManagerCode != null && !managers.ContainsKey(row.ManagerCode))
                errors.Add(new BulkImportValidationError(i, "Manager", row.ManagerCode,
                    $"Manager with code '{row.ManagerCode}' not found", "Warning"));
        }

        return (errors, departments, positions, managers);
    }
}
