using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.BulkImport.Models;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.BulkImport.EmployeeImport.Validation;
using Anemoi.Hr.Domain.Employees;
using OneOf;

namespace Anemoi.Hr.Application.BulkImport.EmployeeImport;

public sealed class EmployeeImportHandler
{
    private readonly ReferenceValidator _referenceValidator;
    private readonly EmployeeImportMappingService _mappingService;
    private readonly ISqlRepository<Employee> _employeeRepo;
    private readonly ISqlRepository<EmployeeHistory> _historyRepo;

    public EmployeeImportHandler(
        ReferenceValidator referenceValidator,
        EmployeeImportMappingService mappingService,
        ISqlRepository<Employee> employeeRepo,
        ISqlRepository<EmployeeHistory> historyRepo)
    {
        _referenceValidator = referenceValidator;
        _mappingService = mappingService;
        _employeeRepo = employeeRepo;
        _historyRepo = historyRepo;
    }

    public async Task<OneOf<IReadOnlyCollection<BulkImportRowResult>, ErrorDetailResponse>> HandleAsync(
        IReadOnlyCollection<EmployeeImportRowDto> validRows,
        string actorUserId,
        CancellationToken ct)
    {
        var (refErrors, departments, positions, managers) =
            await _referenceValidator.ValidateAndResolveAsync(validRows, ct);

        if (refErrors.Any(e => e.Severity == "Error"))
        {
            var results = validRows.Select((_, i) =>
            {
                var rowErrors = refErrors.Where(e => e.RowIndex == i).ToList();
                return new BulkImportRowResult(i,
                    rowErrors.Count > 0 ? BulkImportRowStatus.Failed : BulkImportRowStatus.Success,
                    null, rowErrors.Count > 0 ? rowErrors : null);
            }).ToList().AsReadOnly();

            return results;
        }

        Guid? actorGuid = Guid.TryParse(actorUserId, out var g) ? g : null;
        var employees = new List<Employee>(validRows.Count);
        var histories = new List<EmployeeHistory>(validRows.Count);
        var resultsList = new List<BulkImportRowResult>(validRows.Count);

        foreach (var row in validRows)
        {
            var dept = departments.GetValueOrDefault(row.DepartmentName ?? "");
            var pos = positions.GetValueOrDefault(row.PositionName ?? "");
            var mgr = row.ManagerCode != null ? managers.GetValueOrDefault(row.ManagerCode) : null;

            var employee = _mappingService.MapToEmployee(row, dept!, pos!, mgr);
            var history = _mappingService.MapToHistory(employee, actorGuid);

            employees.Add(employee);
            histories.Add(history);
            resultsList.Add(new BulkImportRowResult(0, BulkImportRowStatus.Success,
                employee.Id.Value.ToString(), null));
        }

        await _employeeRepo.CreateManyAsync(employees, ct);
        await _historyRepo.CreateManyAsync(histories, ct);

        return resultsList.AsReadOnly();
    }
}
