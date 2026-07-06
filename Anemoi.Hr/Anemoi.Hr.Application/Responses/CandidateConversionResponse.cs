using System;

namespace Anemoi.Hr.Application.Responses;

public sealed class CandidateConversionResponse
{
    public string CandidateId { get; set; }
    public string EmployeeId { get; set; }
    public string EmployeeCode { get; set; }
    public bool AlreadyConverted { get; set; }
    public bool RequiresContractCreation { get; set; }
    public DateTime? ConvertedAt { get; set; }
}
