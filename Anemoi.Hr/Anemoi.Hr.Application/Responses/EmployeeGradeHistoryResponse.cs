namespace Anemoi.Hr.Application.Responses;

public sealed class EmployeeGradeHistoryResponse
{
    public string Id { get; set; }
    public string EmployeeId { get; set; }
    public string GradeCode { get; set; }
    public string OldGradeCode { get; set; }
    public DateOnly EffectiveFrom { get; set; }
    public DateOnly? EffectiveTo { get; set; }
    public string ReasonCode { get; set; }
    public string CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
}
