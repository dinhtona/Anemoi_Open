using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.BuildingBlock.Application.Helpers;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Departments;
using Anemoi.Hr.ModelIds.ModelIds;
using OneOf;

namespace Anemoi.Hr.Application.Cqrs.Commands.DepartmentCommands.CreateDepartment;

public sealed class CreateDepartmentHandler(
    ISqlRepository<Department> departmentRepository,
    IUnitOfWork unitOfWork,
    EmployeeMapper mapper)
    : ICommandHandler<CreateDepartmentCommand, OneOf<DepartmentResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<DepartmentResponse, ErrorDetailResponse>> Handle(
        CreateDepartmentCommand request,
        CancellationToken cancellationToken)
    {
        if (!DepartmentTypeCode.IsValid(request.DepartmentTypeCode))
            return HrErrorResponses.Create(HrBusinessErrorCodes.ValDepartmentTypeCodeInvalid);

        var cleanCode = request.Code.Trim();
        var codeExists = await departmentRepository.ExistByConditionAsync(
            x => x.Code == cleanCode,
            cancellationToken);

        if (codeExists)
            return HrErrorResponses.Create(HrBusinessErrorCodes.DepartmentCodeExists);

        var id = new DepartmentId(IdGenerator.NextGuid());
        var department = Department.Create(
            id,
            cleanCode,
            request.Name.Trim(),
            request.DepartmentTypeCode,
            request.ParentDepartmentId,
            request.ManagerEmployeeId);

        await departmentRepository.CreateOneAsync(department, cancellationToken);
        var saveResult = await unitOfWork.SaveChangesAsync(cancellationToken);

        if (saveResult.IsT1)
            return HrErrorResponses.FromSaveResult(saveResult.AsT1, null);

        return mapper.ToDepartmentResponse(department);
    }
}
