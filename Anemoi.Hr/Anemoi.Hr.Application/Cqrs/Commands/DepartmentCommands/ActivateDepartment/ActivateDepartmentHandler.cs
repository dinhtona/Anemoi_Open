using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Departments;
using OneOf;

namespace Anemoi.Hr.Application.Cqrs.Commands.DepartmentCommands.ActivateDepartment;

public sealed class ActivateDepartmentHandler(
    ISqlRepository<Department> departmentRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<ActivateDepartmentCommand, OneOf<SuccessResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<SuccessResponse, ErrorDetailResponse>> Handle(
        ActivateDepartmentCommand request,
        CancellationToken cancellationToken)
    {
        var department = await departmentRepository.GetFirstByConditionAsync(
            x => x.Id == request.Id,
            null,
            cancellationToken);

        if (department is null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.DepartmentNotFound);

        department.Activate();

        var saveResult = await unitOfWork.SaveChangesAsync(cancellationToken);

        if (saveResult.IsT1)
            return HrErrorResponses.FromSaveResult(saveResult.AsT1, null);

        return new SuccessResponse();
    }
}
