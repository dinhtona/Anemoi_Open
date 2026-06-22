using Anemoi.Hr.Application.Cqrs.Common.Dtos;
using Anemoi.Hr.Domain.Transfers;

namespace Anemoi.Hr.Application.Mappings;

public sealed class EmployeeTransferMapper
{
    public EmployeeTransferDto ToDto(EmployeeTransfer transfer)
    {
        if (transfer is null) return null;
        return new EmployeeTransferDto
        {
            Id = transfer.Id.Value.ToString(),
            EmployeeId = transfer.EmployeeId.Value.ToString(),
            EmployeeCode = transfer.Employee?.EmployeeCode,
            EmployeeFullName = transfer.Employee?.FullName,
            SourceDepartmentId = transfer.SourceDepartmentId.Value.ToString(),
            SourceDepartmentName = transfer.SourceDepartment?.Name,
            TargetDepartmentId = transfer.TargetDepartmentId.Value.ToString(),
            TargetDepartmentName = transfer.TargetDepartment?.Name,
            SourcePositionId = transfer.SourcePositionId.Value.ToString(),
            SourcePositionName = transfer.SourcePosition?.Name,
            TargetPositionId = transfer.TargetPositionId.Value.ToString(),
            TargetPositionName = transfer.TargetPosition?.Name,
            StatusCode = transfer.StatusCode,
            EffectiveDate = transfer.EffectiveDate,
            Reason = transfer.Reason,
            CreatedBy = transfer.CreatedBy,
            CreatedAt = transfer.CreatedAt
        };
    }
}
