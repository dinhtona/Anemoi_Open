using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Departments;
using OneOf;

namespace Anemoi.Hr.Application.Cqrs.Commands.DepartmentCommands.UpdateDepartment;

public sealed class UpdateDepartmentHandler(
    ISqlRepository<Department> departmentRepository,
    IUnitOfWork unitOfWork,
    EmployeeMapper mapper)
    : ICommandHandler<UpdateDepartmentCommand, OneOf<DepartmentResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<DepartmentResponse, ErrorDetailResponse>> Handle(
        UpdateDepartmentCommand request,
        CancellationToken cancellationToken)
    {
        var department = await departmentRepository.GetFirstByConditionAsync(
            x => x.Id == request.Id,
            null,
            cancellationToken);

        if (department is null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.DepartmentNotFound);

        var cleanCode = request.Code.Trim();
        var codeExists = await departmentRepository.ExistByConditionAsync(
            x => x.Code == cleanCode && x.Id != request.Id,
            cancellationToken);

        if (codeExists)
            return HrErrorResponses.Create(HrBusinessErrorCodes.DepartmentCodeExists);

        department.UpdateInfo(
            cleanCode,
            request.Name.Trim(),
            request.DepartmentTypeCode,
            request.ParentDepartmentId,
            request.ManagerEmployeeId);

        var saveResult = await unitOfWork.SaveChangesAsync(cancellationToken);

        if (saveResult.IsT1)
            return HrErrorResponses.FromSaveResult(saveResult.AsT1, null);

        return mapper.ToDepartmentResponse(department);
    }
}
