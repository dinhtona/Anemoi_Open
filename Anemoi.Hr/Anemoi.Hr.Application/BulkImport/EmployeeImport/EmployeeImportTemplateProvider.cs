using Anemoi.BuildingBlock.Application.BulkImport.Abstractions;

namespace Anemoi.Hr.Application.BulkImport.EmployeeImport;

public sealed class EmployeeImportTemplateProvider : IBulkImportTemplateProvider
{
    public string TemplateFileName => "employee-import-template.xlsx";

    public IReadOnlyCollection<ImportColumnDefinition> Columns =>
    [
        new("Employee Code", "EmployeeCode", true, "EMP001", "Unique, max 50 chars"),
        new("First Name", "FirstName", true, "John", ""),
        new("Last Name", "LastName", true, "Smith", ""),
        new("Work Email", "WorkEmail", true, "john@company.com", "Must be valid email"),
        new("Personal Email", "PersonalEmail", false, "john@gmail.com", ""),
        new("Phone", "Phone", false, "+84 123 456 789", ""),
        new("Department", "Department", true, "Engineering", "Must match existing department"),
        new("Position", "Position", true, "Software Engineer", "Must match existing position"),
        new("Grade", "Grade", false, "Grade 5", ""),
        new("Manager", "Manager", false, "EMP002", "Existing employee code"),
        new("HireDate", "HireDate", true, "2026-01-15", "Format: yyyy-MM-dd"),
        new("EmploymentType", "EmploymentType", false, "FullTime",
            "FullTime/PartTime/Contract/Intern/Probation"),
        new("Status", "Status", false, "Active",
            "Active/Onboarding/Suspended/Resigned/Terminated/Archived/Draft"),
        new("DOB", "DOB", false, "1990-06-15", "Format: yyyy-MM-dd"),
        new("IdentityUser", "IdentityUser", false, "user@company.com", "Link by email")
    ];
}
