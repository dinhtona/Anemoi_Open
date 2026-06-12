using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Microsoft.EntityFrameworkCore;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Employees;
using Anemoi.Hr.Domain.ShiftManagement;
using Anemoi.Hr.ModelIds.ModelIds;
using OneOf;

namespace Anemoi.Hr.Application.Cqrs.Commands.ShiftManagementCommands.BulkAssignShift;

public sealed class BulkAssignShiftHandler(
    ISqlRepository<EmployeeShiftAssignment> assignmentRepository,
    ISqlRepository<Employee> employeeRepository,
    ISqlRepository<ShiftTemplate> shiftTemplateRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<BulkAssignShiftCommand, OneOf<SuccessResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<SuccessResponse, ErrorDetailResponse>> Handle(
        BulkAssignShiftCommand request,
        CancellationToken cancellationToken)
    {
        var shiftTemplate = await shiftTemplateRepository.GetFirstByConditionAsync(
            x => x.Id == request.ShiftTemplateId, null, cancellationToken);
        if (shiftTemplate is null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.ShiftTemplateNotFound);

        if (!shiftTemplate.IsActive)
            return HrErrorResponses.Create(HrBusinessErrorCodes.ShiftTemplateInactive);

        var uniqueEmployeeIds = request.EmployeeIds
            .Where(e => e is not null)
            .Distinct()
            .ToList();

        if (uniqueEmployeeIds.Count == 0)
            return HrErrorResponses.Create(HrBusinessErrorCodes.EmployeeNotFound);

        var existingEmployeeIds = await employeeRepository.GetQueryable()
            .Where(x => uniqueEmployeeIds.Contains(x.Id))
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);

        var validEmployeeIds = existingEmployeeIds.ToHashSet();

        var existingAssignments = await assignmentRepository.GetQueryable()
            .Where(x => validEmployeeIds.Contains(x.EmployeeId)
                && x.WorkDate == request.WorkDate
                && x.Status == EmployeeShiftAssignmentStatusCode.Assigned)
            .Select(x => x.EmployeeId)
            .ToListAsync(cancellationToken);

        var alreadyAssigned = existingAssignments.ToHashSet();

        var newAssignments = new List<EmployeeShiftAssignment>();
        foreach (var employeeId in validEmployeeIds)
        {
            if (alreadyAssigned.Contains(employeeId))
                continue;

            var assignment = EmployeeShiftAssignment.Create(
                employeeId,
                shiftTemplate,
                request.WorkDate,
                request.AssignedBy);
            newAssignments.Add(assignment);
        }

        if (newAssignments.Count == 0)
            return new SuccessResponse();

        await assignmentRepository.CreateManyAsync(newAssignments, cancellationToken);

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
