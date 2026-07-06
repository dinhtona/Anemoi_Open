using Anemoi.BuildingBlock.Domain;
using Anemoi.Hr.Domain.Employees;
using Anemoi.Hr.Domain.Probation.Events;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Domain.Probation;

public sealed class ProbationRecord : Entity<ProbationRecordId>
{
    public EmployeeId EmployeeId { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public string StatusCode { get; set; }
    public string? Result { get; set; }
    public string? Comment { get; set; }
    public EmployeeId? ReviewerEmployeeId { get; set; }

    public Employee Employee { get; set; }
    public Employee ReviewerEmployee { get; set; }

    public static ProbationRecord Create(ProbationRecordId id, EmployeeId employeeId, DateOnly startDate, DateOnly endDate)
    {
        return new ProbationRecord
        {
            Id = id,
            EmployeeId = employeeId,
            StartDate = startDate,
            EndDate = endDate,
            StatusCode = ProbationStatusCode.Pending
        };
    }

    public void Pass(string result, string comment, EmployeeId reviewerId)
    {
        StatusCode = ProbationStatusCode.Passed;
        Result = result;
        Comment = comment;
        ReviewerEmployeeId = reviewerId;
        AddEvent(new ProbationStatusChangedDomainEvent(Id, EmployeeId, ProbationStatusCode.Pending, ProbationStatusCode.Passed, reviewerId.ToString()));
    }

    public void Fail(string comment, EmployeeId reviewerId)
    {
        StatusCode = ProbationStatusCode.Failed;
        Comment = comment;
        ReviewerEmployeeId = reviewerId;
        AddEvent(new ProbationStatusChangedDomainEvent(Id, EmployeeId, ProbationStatusCode.Pending, ProbationStatusCode.Failed, reviewerId.ToString()));
    }

    public void Extend(DateOnly newEndDate)
    {
        var oldEndDate = EndDate;
        EndDate = newEndDate;
        StatusCode = ProbationStatusCode.Extended;
        AddEvent(new ProbationStatusChangedDomainEvent(Id, EmployeeId, ProbationStatusCode.Pending, ProbationStatusCode.Extended, "system"));
    }
}
