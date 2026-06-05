using System;

namespace Anemoi.Hr.Application.Responses;

public sealed class EmployeeContractResponse
{
    public string Id { get; set; }
    public string EmployeeId { get; set; }
    public string ContractNumber { get; set; }
    public string ContractTypeCode { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public DateOnly SignedDate { get; set; }
    public string StatusCode { get; set; }
    public string Notes { get; set; }
    public string AttachmentFileId { get; set; }
    public string PreviousContractId { get; set; }
    public string CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public string UpdatedBy { get; set; }
    public DateTime UpdatedAt { get; set; }
}
