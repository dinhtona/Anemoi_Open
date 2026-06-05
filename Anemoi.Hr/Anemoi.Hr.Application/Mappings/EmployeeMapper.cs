using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Employees;
using Anemoi.Hr.Domain.Departments;
using Anemoi.Hr.Domain.Positions;
using Riok.Mapperly.Abstractions;

namespace Anemoi.Hr.Application.Mappings;

[Mapper]
public partial class EmployeeMapper
{
    public EmployeeResponse ToEmployeeResponse(Employee employee)
    {
        if (employee is null) return null;
        return new EmployeeResponse
        {
            Id = employee.Id.Value.ToString(),
            EmployeeCode = employee.EmployeeCode,
            FullName = employee.FullName,
            WorkEmail = employee.WorkEmail,
            PersonalEmail = employee.PersonalEmail,
            PhoneNumber = employee.PhoneNumber,
            DateOfBirth = employee.DateOfBirth,
            JoinDate = employee.JoinDate,
            EmploymentStatusCode = employee.EmploymentStatusCode,
            EmploymentTypeCode = employee.EmploymentTypeCode,
            PrimaryDepartmentId = employee.PrimaryDepartmentId?.Value.ToString(),
            PrimaryPositionId = employee.PrimaryPositionId?.Value.ToString(),
            DirectManagerEmployeeId = employee.DirectManagerEmployeeId?.Value.ToString(),
            CreatedAt = employee.CreatedAt,
            UpdatedAt = employee.UpdatedAt,
            PrimaryDepartment = ToDepartmentResponse(employee.PrimaryDepartment),
            PrimaryPosition = ToPositionResponse(employee.PrimaryPosition),
            DirectManager = ToEmployeeResponse(employee.DirectManager)
        };
    }

    public DepartmentResponse ToDepartmentResponse(Department department)
    {
        if (department is null) return null;
        return new DepartmentResponse
        {
            Id = department.Id.Value.ToString(),
            Code = department.Code,
            Name = department.Name,
            DepartmentTypeCode = department.DepartmentTypeCode,
            ParentDepartmentId = department.ParentDepartmentId?.Value.ToString(),
            ManagerEmployeeId = department.ManagerEmployeeId?.Value.ToString(),
            IsActive = department.IsActive,
            CreatedAt = department.CreatedAt,
            UpdatedAt = department.UpdatedAt
        };
    }

    public PositionResponse ToPositionResponse(Position position)
    {
        if (position is null) return null;
        return new PositionResponse
        {
            Id = position.Id.Value.ToString(),
            DepartmentId = position.DepartmentId.Value.ToString(),
            Code = position.Code,
            Name = position.Name,
            PositionTypeCode = position.PositionTypeCode,
            IsActive = position.IsActive,
            CreatedAt = position.CreatedAt,
            UpdatedAt = position.UpdatedAt
        };
    }
}
