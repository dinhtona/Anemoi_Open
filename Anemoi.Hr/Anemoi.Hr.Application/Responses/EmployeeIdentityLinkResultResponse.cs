namespace Anemoi.Hr.Application.Responses;

public sealed class EmployeeIdentityLinkResultResponse
{
    public int TotalEvaluated { get; set; }
    public int Matched { get; set; }
    public int NoMatch { get; set; }
    public int AlreadyLinked { get; set; }
    public int ConflictExistingMapping { get; set; }
    public int DuplicateEmployeeEmail { get; set; }
    public int DuplicateIdentityEmail { get; set; }
    public int InvalidEmail { get; set; }
    public int Skipped { get; set; }
}
