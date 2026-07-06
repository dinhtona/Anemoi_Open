using Anemoi.BuildingBlock.Application.Helpers;
using Anemoi.Hr.Domain.Departments;
using Anemoi.Hr.Domain.Employees;
using Anemoi.Hr.Domain.Positions;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Application.BulkImport.EmployeeImport;

public sealed class EmployeeImportMappingService
{
    public Employee MapToEmployee(
        EmployeeImportRowDto row,
        Department department,
        Position position,
        Employee? manager)
    {
        var now = DateTime.UtcNow;
        var employeeId = new EmployeeId(IdGenerator.NextGuid());

        DateOnly? dob = null;
        if (DateOnly.TryParse(row.DateOfBirthStr, out var parsedDob))
            dob = parsedDob;

        var joinDate = DateOnly.FromDateTime(DateTime.UtcNow);
        if (DateOnly.TryParse(row.HireDateStr, out var parsedHire))
            joinDate = parsedHire;

        return new Employee
        {
            Id = employeeId,
            EmployeeCode = row.EmployeeCode,
            FirstName = row.FirstName,
            LastName = row.LastName,
            WorkEmail = row.WorkEmail,
            PersonalEmail = row.PersonalEmail ?? string.Empty,
            PhoneNumber = row.Phone ?? string.Empty,
            DateOfBirth = dob,
            JoinDate = joinDate,
            EmploymentStatusCode = EmploymentStatusCode.Draft,
            EmploymentTypeCode = row.EmploymentType ?? "FullTime",
            GradeCode = row.GradeName,
            PrimaryDepartmentId = department.Id,
            PrimaryPositionId = position.Id,
            DirectManagerEmployeeId = manager?.Id,
            CreatedAt = now,
            UpdatedAt = now
        };
    }

    public EmployeeHistory MapToHistory(Employee employee, Guid? actorGuid)
    {
        return new EmployeeHistory
        {
            Id = new EmployeeHistoryId(IdGenerator.NextGuid()),
            EmployeeId = employee.Id,
            EntityType = "Employee",
            EntityId = employee.Id.Value.ToString(),
            EventType = "BulkImported",
            Title = "Bulk import",
            Description = $"{employee.FullName} ({employee.EmployeeCode})",
            OccurredAt = DateTime.UtcNow,
            ActorUserId = actorGuid
        };
    }
}
