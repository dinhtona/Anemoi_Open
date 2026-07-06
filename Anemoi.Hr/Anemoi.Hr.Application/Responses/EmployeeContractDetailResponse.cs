using System;

namespace Anemoi.Hr.Application.Responses;

public sealed class EmployeeContractDetailResponse
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

    // Termination details (flattened for ease of use)
    public DateOnly? TerminatedDate { get; set; }
    public string TerminationReasonCode { get; set; }
    public string TerminationNotes { get; set; }
    public string TerminationAttachmentId { get; set; }
}
