using System;

namespace Anemoi.Hr.Application.Responses;

public sealed class JobPostingResponse
{
    public string Id { get; set; }
    public string JobRequisitionId { get; set; }
    public string RequisitionCode { get; set; }
    public string RequisitionTitle { get; set; }
    public string PostingTitle { get; set; }
    public string PostingDescription { get; set; }
    public string PublishDate { get; set; }
    public string ExpiryDate { get; set; }
    public string Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string UpdatedBy { get; set; }
    public DateTime? PublishedAt { get; set; }
    public string PublishedBy { get; set; }
    public DateTime? ExpiredAt { get; set; }
    public string ExpiredBy { get; set; }
    public DateTime? ClosedAt { get; set; }
    public string ClosedBy { get; set; }
}
