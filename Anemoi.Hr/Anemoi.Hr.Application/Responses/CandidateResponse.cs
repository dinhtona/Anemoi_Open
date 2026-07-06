using System;

namespace Anemoi.Hr.Application.Responses;

public sealed class CandidateResponse
{
    public string Id { get; set; }
    public string CandidateCode { get; set; }
    public string FullName { get; set; }
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public string DateOfBirth { get; set; }
    public string Address { get; set; }
    public string ResumeUrl { get; set; }
    public string Source { get; set; }
    public string Status { get; set; }
    public string Notes { get; set; }
    public string EmployeeId { get; set; }
    public DateTime? ConvertedAt { get; set; }
    public string ConvertedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string UpdatedBy { get; set; }
}
