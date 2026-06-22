namespace Anemoi.Hr.Application.Cqrs.Common.Dtos;

public sealed record EmployeeTransferDto
{
    public string Id { get; set; }
    public string EmployeeId { get; set; }
    public string EmployeeCode { get; set; }
    public string EmployeeFullName { get; set; }
    public string SourceDepartmentId { get; set; }
    public string SourceDepartmentName { get; set; }
    public string TargetDepartmentId { get; set; }
    public string TargetDepartmentName { get; set; }
    public string SourcePositionId { get; set; }
    public string SourcePositionName { get; set; }
    public string TargetPositionId { get; set; }
    public string TargetPositionName { get; set; }
    public string StatusCode { get; set; }
    public DateOnly EffectiveDate { get; set; }
    public string? Reason { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
}
