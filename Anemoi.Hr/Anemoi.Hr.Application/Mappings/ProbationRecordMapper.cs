using Anemoi.Hr.Application.Cqrs.Common.Dtos;
using Anemoi.Hr.Domain.Probation;

namespace Anemoi.Hr.Application.Mappings;

public sealed class ProbationRecordMapper
{
    public ProbationRecordDto ToDto(ProbationRecord record)
    {
        if (record is null) return null;
        return new ProbationRecordDto
        {
            Id = record.Id.Value.ToString(),
            EmployeeId = record.EmployeeId.Value.ToString(),
            EmployeeCode = record.Employee?.EmployeeCode,
            EmployeeFullName = record.Employee?.FullName,
            StartDate = record.StartDate,
            EndDate = record.EndDate,
            StatusCode = record.StatusCode,
            Result = record.Result,
            Comment = record.Comment,
            ReviewerEmployeeId = record.ReviewerEmployeeId?.Value.ToString(),
            ReviewerName = record.ReviewerEmployee?.FullName
        };
    }
}
