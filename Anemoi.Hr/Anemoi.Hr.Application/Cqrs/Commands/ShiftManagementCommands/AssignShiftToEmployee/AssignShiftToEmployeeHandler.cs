using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Microsoft.EntityFrameworkCore;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Employees;
using Anemoi.Hr.Domain.ShiftManagement;
using OneOf;

namespace Anemoi.Hr.Application.Cqrs.Commands.ShiftManagementCommands.AssignShiftToEmployee;

public sealed class AssignShiftToEmployeeHandler(
    ISqlRepository<EmployeeShiftAssignment> assignmentRepository,
    ISqlRepository<Employee> employeeRepository,
    ISqlRepository<ShiftTemplate> shiftTemplateRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<AssignShiftToEmployeeCommand, OneOf<SuccessResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<SuccessResponse, ErrorDetailResponse>> Handle(
        AssignShiftToEmployeeCommand request,
        CancellationToken cancellationToken)
    {
        var employee = await employeeRepository.GetFirstByConditionAsync(
            x => x.Id == request.EmployeeId, null, cancellationToken);
        if (employee is null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.EmployeeNotFound);

        var shiftTemplate = await shiftTemplateRepository.GetFirstByConditionAsync(
            x => x.Id == request.ShiftTemplateId, null, cancellationToken);
        if (shiftTemplate is null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.ShiftTemplateNotFound);

        if (!shiftTemplate.IsActive)
            return HrErrorResponses.Create(HrBusinessErrorCodes.ShiftTemplateInactive);

        var existingActive = await assignmentRepository.GetQueryable()
            .AnyAsync(x => x.EmployeeId == request.EmployeeId
                && x.WorkDate == request.WorkDate
                && x.Status == EmployeeShiftAssignmentStatusCode.Assigned, cancellationToken);
        if (existingActive)
            return HrErrorResponses.Create(HrBusinessErrorCodes.ShiftAssignmentDuplicate);

        var assignment = EmployeeShiftAssignment.Create(
            request.EmployeeId,
            shiftTemplate,
            request.WorkDate,
            request.AssignedBy);

        await assignmentRepository.CreateOneAsync(assignment, cancellationToken);

        var saveResult = await unitOfWork.SaveChangesAsync(cancellationToken);
        if (saveResult.IsT1)
        {
            return saveResult.AsT1 is DbUpdateConcurrencyException
                ? HrErrorResponses.Create(HrBusinessErrorCodes.ShiftAssignmentConcurrencyConflict)
                : HrErrorResponses.Create("HR_SAVE_CHANGES_FAILED");
        }

        return new SuccessResponse();
    }
}
