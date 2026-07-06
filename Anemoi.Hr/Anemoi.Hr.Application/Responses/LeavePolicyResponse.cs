namespace Anemoi.Hr.Application.Responses;

public sealed class LeavePolicyResponse
{
    public string Id { get; set; }
    public string Code { get; set; }
    public string Name { get; set; }
    public string LeaveTypeId { get; set; }
    public string LeaveTypeCode { get; set; }
    public string LeaveTypeName { get; set; }
    public string ApplicableGradeCode { get; set; }
    public decimal AnnualEntitlement { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
