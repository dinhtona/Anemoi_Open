using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.BuildingBlock.Application.Helpers;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Employees;
using Anemoi.Hr.Domain.Workflow;
using Anemoi.Hr.ModelIds.ModelIds;
using OneOf;

namespace Anemoi.Hr.Application.Cqrs.Commands.WorkflowCommands.AssignWorkflowRole;

public sealed class AssignWorkflowRoleHandler(
    ISqlRepository<WorkflowRoleAssignment> assignmentRepository,
    ISqlRepository<Employee> employeeRepository,
    IUnitOfWork unitOfWork,
    WorkflowRoleAssignmentMapper mapper)
    : ICommandHandler<AssignWorkflowRoleCommand, OneOf<WorkflowRoleAssignmentResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<WorkflowRoleAssignmentResponse, ErrorDetailResponse>> Handle(
        AssignWorkflowRoleCommand request, CancellationToken cancellationToken)
    {
        var employeeExists = await employeeRepository.ExistByConditionAsync(
            e => e.Id == request.EmployeeId, cancellationToken);
        if (!employeeExists)
            return HrErrorResponses.Create(HrBusinessErrorCodes.EmployeeNotFound);

        var duplicateExists = await assignmentRepository.ExistByConditionAsync(
            a => a.Role == request.Role && a.EmployeeId == request.EmployeeId, cancellationToken);
        if (duplicateExists)
            return HrErrorResponses.Create(HrBusinessErrorCodes.WorkflowRoleAssignmentDuplicate);

        var id = new WorkflowRoleAssignmentId(IdGenerator.NextGuid());
        var assignment = WorkflowRoleAssignment.Create(id, request.Role, request.EmployeeId);

        var createResult = await assignmentRepository.CreateOneAsync(assignment, cancellationToken);
        if (createResult.TryPickT1(out var exception, out _))
            return HrErrorResponses.FromSaveResult(exception, HrBusinessErrorCodes.SaveChangesFailed);

        var saveResult = await unitOfWork.SaveChangesAsync(cancellationToken);
        if (saveResult.TryPickT1(out var saveException, out _))
            return HrErrorResponses.FromSaveResult(saveException, HrBusinessErrorCodes.SaveChangesFailed);

        return mapper.ToResponse(assignment);
    }
}
