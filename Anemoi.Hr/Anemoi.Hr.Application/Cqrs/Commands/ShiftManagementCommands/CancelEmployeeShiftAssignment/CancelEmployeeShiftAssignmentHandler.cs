using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Microsoft.EntityFrameworkCore;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.ShiftManagement;
using OneOf;

namespace Anemoi.Hr.Application.Cqrs.Commands.ShiftManagementCommands.CancelEmployeeShiftAssignment;

public sealed class CancelEmployeeShiftAssignmentHandler(
    ISqlRepository<EmployeeShiftAssignment> assignmentRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<CancelEmployeeShiftAssignmentCommand, OneOf<SuccessResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<SuccessResponse, ErrorDetailResponse>> Handle(
        CancelEmployeeShiftAssignmentCommand request,
        CancellationToken cancellationToken)
    {
        var assignment = await assignmentRepository.GetFirstByConditionAsync(
            x => x.Id == request.Id, null, cancellationToken);
        if (assignment is null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.ShiftAssignmentNotFound);

        try
        {
            assignment.Cancel(request.CancelledBy, request.CancellationReason);
        }
        catch (InvalidOperationException)
        {
            return HrErrorResponses.Create(HrBusinessErrorCodes.ShiftAssignmentInvalidStatus);
        }

        var saveResult = await unitOfWork.SaveChangesAsync(cancellationToken);
        if (saveResult.IsT1)
            return HrErrorResponses.FromSaveResult(saveResult.AsT1, HrBusinessErrorCodes.ShiftAssignmentConcurrencyConflict);

        return new SuccessResponse();
    }
}
