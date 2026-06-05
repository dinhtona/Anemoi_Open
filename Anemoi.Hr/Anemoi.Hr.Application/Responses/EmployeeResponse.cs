using System;

namespace Anemoi.Hr.Application.Responses;

public sealed class EmployeeResponse
{
    public string Id { get; set; }
    public string EmployeeCode { get; set; }
    public string FullName { get; set; }
    public string WorkEmail { get; set; }
    public string PersonalEmail { get; set; }
    public string PhoneNumber { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public DateOnly JoinDate { get; set; }
    public string EmploymentStatusCode { get; set; }
    public string EmploymentTypeCode { get; set; }
    public string GradeCode { get; set; }
    public string PrimaryDepartmentId { get; set; }
    public string PrimaryPositionId { get; set; }
    public string DirectManagerEmployeeId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public DepartmentResponse PrimaryDepartment { get; set; }
    public PositionResponse PrimaryPosition { get; set; }
    public EmployeeResponse DirectManager { get; set; }
}
