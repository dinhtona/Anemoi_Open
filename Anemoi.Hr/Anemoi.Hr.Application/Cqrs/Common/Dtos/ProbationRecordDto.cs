namespace Anemoi.Hr.Application.Cqrs.Common.Dtos;

public sealed record ProbationRecordDto
{
    public string Id { get; set; }
    public string EmployeeId { get; set; }
    public string EmployeeCode { get; set; }
    public string EmployeeFullName { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public string StatusCode { get; set; }
    public string? Result { get; set; }
    public string? Comment { get; set; }
    public string? ReviewerEmployeeId { get; set; }
    public string? ReviewerName { get; set; }
    public DateTime CreatedAt { get; set; }
}
