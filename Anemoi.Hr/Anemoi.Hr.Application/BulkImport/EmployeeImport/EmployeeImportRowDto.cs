namespace Anemoi.Hr.Application.BulkImport.EmployeeImport;

public sealed record EmployeeImportRowDto
{
    public Dictionary<string, string> RawData { get; init; } = new();

    public string EmployeeCode => RawData.GetValueOrDefault("EmployeeCode", "");
    public string FirstName => RawData.GetValueOrDefault("FirstName", "");
    public string LastName => RawData.GetValueOrDefault("LastName", "");
    public string WorkEmail => RawData.GetValueOrDefault("WorkEmail", "");
    public string? PersonalEmail => RawData.GetValueOrDefault("PersonalEmail");
    public string? Phone => RawData.GetValueOrDefault("Phone");
    public string? DepartmentName => RawData.GetValueOrDefault("Department");
    public string? PositionName => RawData.GetValueOrDefault("Position");
    public string? GradeName => RawData.GetValueOrDefault("Grade");
    public string? ManagerCode => RawData.GetValueOrDefault("Manager");
    public string? HireDateStr => RawData.GetValueOrDefault("HireDate");
    public string? EmploymentType => RawData.GetValueOrDefault("EmploymentType");
    public string? StatusValue => RawData.GetValueOrDefault("Status");
    public string? DateOfBirthStr => RawData.GetValueOrDefault("DOB");
    public string? IdentityUserEmail => RawData.GetValueOrDefault("IdentityUser");
}
