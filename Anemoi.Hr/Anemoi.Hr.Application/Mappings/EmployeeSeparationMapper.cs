using Anemoi.Hr.Application.Cqrs.Common.Dtos;
using Anemoi.Hr.Domain.Separations;

namespace Anemoi.Hr.Application.Mappings;

public sealed class EmployeeSeparationMapper
{
    public EmployeeSeparationDto ToDto(EmployeeSeparation separation)
    {
        if (separation is null) return null;
        return new EmployeeSeparationDto
        {
            Id = separation.Id.Value.ToString(),
            EmployeeId = separation.EmployeeId.Value.ToString(),
            EmployeeCode = separation.Employee?.EmployeeCode,
            EmployeeFullName = separation.Employee?.FullName,
            SeparationTypeCode = separation.SeparationTypeCode,
            StatusCode = separation.StatusCode,
            SeparationDate = separation.SeparationDate,
            LastWorkingDate = separation.LastWorkingDate,
            Reason = separation.Reason,
            Details = separation.Details,
            CreatedBy = separation.CreatedBy,
            CreatedAt = separation.CreatedAt
        };
    }
}
