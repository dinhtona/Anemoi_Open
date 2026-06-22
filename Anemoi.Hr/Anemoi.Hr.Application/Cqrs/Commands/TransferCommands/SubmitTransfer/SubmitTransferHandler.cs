using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.BuildingBlock.Application.Helpers;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Contract.Identity.ModelIds;
using Anemoi.Hr.Application.Abstractions;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Cqrs.Common.Dtos;
using Anemoi.Hr.Domain.Departments;
using Anemoi.Hr.Domain.Employees;
using Anemoi.Hr.Domain.Positions;
using Anemoi.Hr.Domain.Transfers;
using Anemoi.Hr.ModelIds.ModelIds;
using OneOf;

namespace Anemoi.Hr.Application.Cqrs.Commands.TransferCommands.SubmitTransfer;

public sealed class SubmitTransferHandler(
    ISqlRepository<Employee> employeeRepository,
    ISqlRepository<Department> departmentRepository,
    ISqlRepository<Position> positionRepository,
    ISqlRepository<EmployeeTransfer> transferRepository,
    IUnitOfWork unitOfWork,
    IWorkflowEngine workflowEngine,
    ICurrentUser currentUser,
    Mappings.EmployeeTransferMapper mapper)
    : ICommandHandler<SubmitTransferCommand, OneOf<EmployeeTransferDto, ErrorDetailResponse>>
{
    public async Task<OneOf<EmployeeTransferDto, ErrorDetailResponse>> Handle(
        SubmitTransferCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var employee = await employeeRepository.GetFirstByConditionAsync(
                x => x.Id == request.EmployeeId, null, cancellationToken);
            if (employee == null)
                return HrErrorResponses.Create(HrBusinessErrorCodes.EmployeeNotFound);

            var targetDepartment = await departmentRepository.GetFirstByConditionAsync(
                x => x.Id == request.ToDepartmentId, null, cancellationToken);
            if (targetDepartment == null)
                return HrErrorResponses.Create(HrBusinessErrorCodes.DepartmentNotFound);

            var targetPosition = await positionRepository.GetFirstByConditionAsync(
                x => x.Id == request.ToPositionId, null, cancellationToken);
            if (targetPosition == null)
                return HrErrorResponses.Create(HrBusinessErrorCodes.PositionNotFound);

            var id = new EmployeeTransferId(IdGenerator.NextGuid());
            var transfer = EmployeeTransfer.Create(
                id,
                request.EmployeeId,
                employee.PrimaryDepartmentId,
                request.ToDepartmentId,
                employee.PrimaryPositionId,
                request.ToPositionId,
                request.EffectiveDate,
                request.Reason,
                currentUser.UserId);

            await transferRepository.CreateOneAsync(transfer, cancellationToken);

            var saveResult = await unitOfWork.SaveChangesAsync(cancellationToken);
            if (saveResult.IsT1)
                return HrErrorResponses.FromSaveResult(saveResult.AsT1, HrBusinessErrorCodes.SaveChangesFailed);

            var requesterUserId = new UserId(Guid.Parse(currentUser.UserId));
            var requesterEmployeeId = new EmployeeId(Guid.Parse(currentUser.UserId));
            await workflowEngine.StartAsync(
                WorkflowConstants.TargetEntityTypes.EmployeeTransfer,
                transfer.Id.Value,
                requesterEmployeeId,
                requesterUserId,
                requesterUserId,
                cancellationToken);

            return mapper.ToDto(transfer);
        }
        catch (Exception exception)
        {
            return HrErrorResponses.FromSaveResult(exception, HrBusinessErrorCodes.SaveChangesFailed);
        }
    }
}
