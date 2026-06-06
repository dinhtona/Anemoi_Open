using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Compensation;
using Riok.Mapperly.Abstractions;
using System.Linq;

namespace Anemoi.Hr.Application.Mappings;

[Mapper]
public partial class CompensationMapper
{
    public EmployeeSalaryResponse ToEmployeeSalaryResponse(EmployeeSalary entity)
    {
        if (entity is null) return null;
        return new EmployeeSalaryResponse
        {
            Id = entity.Id.Value.ToString(),
            EmployeeId = entity.EmployeeId.Value.ToString(),
            SalaryGradeId = entity.SalaryGradeId?.Value.ToString(),
            GradeCodeSnapshot = entity.GradeCodeSnapshot,
            BaseSalary = entity.BaseSalary,
            SalaryType = entity.SalaryType,
            Currency = entity.Currency,
            EffectiveFrom = entity.EffectiveFrom,
            EffectiveTo = entity.EffectiveTo,
            Reason = entity.Reason,
            CreatedBy = entity.CreatedBy,
            CreatedAt = entity.CreatedAt
        };
    }

    public EmployeeAllowanceResponse ToEmployeeAllowanceResponse(EmployeeAllowance entity)
    {
        if (entity is null) return null;
        return new EmployeeAllowanceResponse
        {
            Id = entity.Id.Value.ToString(),
            EmployeeId = entity.EmployeeId.Value.ToString(),
            AllowanceTypeId = entity.AllowanceTypeId.Value.ToString(),
            AllowanceTypeCode = entity.AllowanceType?.Code,
            AllowanceTypeName = entity.AllowanceType?.Name,
            Amount = entity.Amount,
            Currency = entity.Currency,
            EffectiveFrom = entity.EffectiveFrom,
            EffectiveTo = entity.EffectiveTo,
            CreatedBy = entity.CreatedBy,
            CreatedAt = entity.CreatedAt,
            UpdatedBy = entity.UpdatedBy,
            UpdatedAt = entity.UpdatedAt
        };
    }

    public PositionAllowanceResponse ToPositionAllowanceResponse(PositionAllowance entity)
    {
        if (entity is null) return null;
        return new PositionAllowanceResponse
        {
            Id = entity.Id.Value.ToString(),
            PositionId = entity.PositionId.Value.ToString(),
            AllowanceTypeId = entity.AllowanceTypeId.Value.ToString(),
            AllowanceTypeCode = entity.AllowanceType?.Code,
            AllowanceTypeName = entity.AllowanceType?.Name,
            Amount = entity.Amount,
            Currency = entity.Currency,
            IsActive = entity.IsActive,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }

    public SalaryRangeResponse ToSalaryRangeResponse(SalaryRange entity)
    {
        if (entity is null) return null;
        return new SalaryRangeResponse
        {
            Id = entity.Id.Value.ToString(),
            SalaryGradeId = entity.SalaryGradeId.Value.ToString(),
            MinSalary = entity.MinSalary,
            MaxSalary = entity.MaxSalary,
            Currency = entity.Currency,
            EffectiveFrom = entity.EffectiveFrom,
            EffectiveTo = entity.EffectiveTo,
            IsActive = entity.IsActive
        };
    }

    public SalaryGradeResponse ToSalaryGradeResponse(SalaryGrade entity)
    {
        if (entity is null) return null;
        return new SalaryGradeResponse
        {
            Id = entity.Id.Value.ToString(),
            GradeCode = entity.GradeCode,
            Name = entity.Name,
            Description = entity.Description,
            IsActive = entity.IsActive,
            Ranges = entity.Ranges?.Select(ToSalaryRangeResponse).ToList() ?? []
        };
    }

    public SalaryValidationBypassLogResponse ToSalaryValidationBypassLogResponse(SalaryValidationBypassLog entity)
    {
        if (entity is null) return null;
        return new SalaryValidationBypassLogResponse
        {
            Id = entity.Id.Value.ToString(),
            EmployeeId = entity.EmployeeId.Value.ToString(),
            GradeCode = entity.GradeCode,
            RequestedSalary = entity.RequestedSalary,
            Currency = entity.Currency,
            BypassReason = entity.BypassReason,
            CreatedBy = entity.CreatedBy,
            CreatedAt = entity.CreatedAt
        };
    }

    public AllowanceTypeResponse ToAllowanceTypeResponse(AllowanceType entity)
    {
        if (entity is null) return null;
        return new AllowanceTypeResponse
        {
            Id = entity.Id.Value.ToString(),
            Code = entity.Code,
            Name = entity.Name,
            Description = entity.Description,
            IsTaxable = entity.IsTaxable,
            IsActive = entity.IsActive
        };
    }
}
