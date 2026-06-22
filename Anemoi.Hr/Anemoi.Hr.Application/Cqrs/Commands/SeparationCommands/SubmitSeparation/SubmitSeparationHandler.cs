using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.BuildingBlock.Application.Helpers;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Contract.Identity.ModelIds;
using Anemoi.Hr.Application.Abstractions;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Cqrs.Common.Dtos;
using Anemoi.Hr.Domain.Employees;
using Anemoi.Hr.Domain.Separations;
using Anemoi.Hr.ModelIds.ModelIds;
using OneOf;

namespace Anemoi.Hr.Application.Cqrs.Commands.SeparationCommands.SubmitSeparation;

public sealed class SubmitSeparationHandler(
    ISqlRepository<Employee> employeeRepository,
    ISqlRepository<EmployeeSeparation> separationRepository,
    IUnitOfWork unitOfWork,
    IWorkflowEngine workflowEngine,
    ICurrentUser currentUser,
    Mappings.EmployeeSeparationMapper mapper)
    : ICommandHandler<SubmitSeparationCommand, OneOf<EmployeeSeparationDto, ErrorDetailResponse>>
{
    public async Task<OneOf<EmployeeSeparationDto, ErrorDetailResponse>> Handle(
        SubmitSeparationCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var employee = await employeeRepository.GetFirstByConditionAsync(
                x => x.Id == request.EmployeeId, null, cancellationToken);
            if (employee == null)
                return HrErrorResponses.Create(HrBusinessErrorCodes.EmployeeNotFound);

            var id = new EmployeeSeparationId(IdGenerator.NextGuid());
            var separation = EmployeeSeparation.Create(
                id,
                request.EmployeeId,
                request.SeparationType,
                request.LastWorkingDate,
                request.LastWorkingDate,
                request.Reason,
                currentUser.UserId);

            await separationRepository.CreateOneAsync(separation, cancellationToken);

            var saveResult = await unitOfWork.SaveChangesAsync(cancellationToken);
            if (saveResult.IsT1)
                return HrErrorResponses.FromSaveResult(saveResult.AsT1, HrBusinessErrorCodes.SaveChangesFailed);

            var requesterUserId = new UserId(Guid.Parse(currentUser.UserId));
            var requesterEmployeeId = new EmployeeId(Guid.Parse(currentUser.UserId));
            await workflowEngine.StartAsync(
                WorkflowConstants.TargetEntityTypes.EmployeeSeparation,
                separation.Id.Value,
                requesterEmployeeId,
                requesterUserId,
                requesterUserId,
                cancellationToken);

            return mapper.ToDto(separation);
        }
        catch (Exception exception)
        {
            return HrErrorResponses.FromSaveResult(exception, HrBusinessErrorCodes.SaveChangesFailed);
        }
    }
}
