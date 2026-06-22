namespace Anemoi.Hr.Application.Cqrs.Common.Dtos;

public sealed record EmployeeSeparationDto
{
    public string Id { get; set; }
    public string EmployeeId { get; set; }
    public string EmployeeCode { get; set; }
    public string EmployeeFullName { get; set; }
    public string SeparationTypeCode { get; set; }
    public string StatusCode { get; set; }
    public DateOnly SeparationDate { get; set; }
    public DateOnly? LastWorkingDate { get; set; }
    public string? Reason { get; set; }
    public string? Details { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
}
