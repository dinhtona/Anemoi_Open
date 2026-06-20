using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Departments;
using Anemoi.Hr.Domain.Employees;
using Anemoi.Hr.Domain.Positions;
using Microsoft.EntityFrameworkCore;
using OneOf;

namespace Anemoi.Hr.Application.Cqrs.Commands.DepartmentCommands.DeactivateDepartment;

public sealed class DeactivateDepartmentHandler(
    ISqlRepository<Department> departmentRepository,
    ISqlRepository<Employee> employeeRepository,
    ISqlRepository<Position> positionRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<DeactivateDepartmentCommand, OneOf<SuccessResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<SuccessResponse, ErrorDetailResponse>> Handle(
        DeactivateDepartmentCommand request,
        CancellationToken cancellationToken)
    {
        var department = await departmentRepository.GetFirstByConditionAsync(
            x => x.Id == request.Id,
            null,
            cancellationToken);

        if (department is null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.DepartmentNotFound);

        var hasChildren = await departmentRepository.ExistByConditionAsync(
            x => x.ParentDepartmentId == request.Id && x.IsActive,
            cancellationToken);

        if (hasChildren)
            return HrErrorResponses.Create(HrBusinessErrorCodes.DepartmentHasChildren);

        var hasActiveEmployees = await employeeRepository.ExistByConditionAsync(
            x => x.PrimaryDepartmentId == request.Id,
            cancellationToken);

        if (hasActiveEmployees)
            return HrErrorResponses.Create(HrBusinessErrorCodes.DepartmentHasActiveEmployees);

        var hasActivePositions = await positionRepository.ExistByConditionAsync(
            x => x.DepartmentId == request.Id && x.IsActive,
            cancellationToken);

        if (hasActivePositions)
            return HrErrorResponses.Create(HrBusinessErrorCodes.DepartmentHasActivePositions);

        department.Deactivate();

        var saveResult = await unitOfWork.SaveChangesAsync(cancellationToken);

        if (saveResult.IsT1)
            return HrErrorResponses.FromSaveResult(saveResult.AsT1, null);

        return new SuccessResponse();
    }
}
