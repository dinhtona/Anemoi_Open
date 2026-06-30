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
        var employee = await employeeRepository.GetFirstByConditionAsync(
            x => x.Id == request.EmployeeId, null, cancellationToken);
        if (employee == null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.EmployeeNotFound);

        var id = new EmployeeSeparationId(IdGenerator.NextGuid());
        // SeparationDate equals LastWorkingDate in the current model.
        // These are distinct concepts (decision date vs effective date)
        // but the command omits SeparationDate — when needed, add it to
        // SubmitSeparationCommand and pass it separately here.
        var separation = EmployeeSeparation.Create(
            id,
            request.EmployeeId,
            request.SeparationType,
            separationDate: request.LastWorkingDate,
            lastWorkingDate: request.LastWorkingDate,
            request.Reason,
            currentUser.UserId);

        await separationRepository.CreateOneAsync(separation, cancellationToken);

        var requesterUserId = new UserId(Guid.Parse(currentUser.UserId));
        var workflowResult = await workflowEngine.StartAsync(
            WorkflowConstants.TargetEntityTypes.EmployeeSeparation,
            separation.Id.Value,
            employee.Id,
            requesterUserId,
            requesterUserId,
            cancellationToken);

        if (workflowResult.TryPickT1(out var workflowError, out _))
            return workflowError;

        var saveResult = await unitOfWork.SaveChangesAsync(cancellationToken);
        if (saveResult.IsT1)
            return HrErrorResponses.FromSaveResult(saveResult.AsT1, HrBusinessErrorCodes.SaveChangesFailed);

        return mapper.ToDto(separation);
    }
}
