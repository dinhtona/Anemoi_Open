using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.BuildingBlock.Application.Responses;
using Microsoft.EntityFrameworkCore;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Contract.Hr.Events;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Overtime;
using MassTransit;
using OneOf;

namespace Anemoi.Hr.Application.Cqrs.Commands.OvertimeRequestCommands.ApproveOvertimeRequest;

public sealed class ApproveOvertimeRequestHandler(
    ISqlRepository<OvertimeRequest> overtimeRequestRepository,
    IUnitOfWork unitOfWork,
    IPublishEndpoint publishEndpoint)
    : ICommandHandler<ApproveOvertimeRequestCommand, OneOf<SuccessResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<SuccessResponse, ErrorDetailResponse>> Handle(ApproveOvertimeRequestCommand request,
        CancellationToken cancellationToken)
    {
        var overtimeRequest = await overtimeRequestRepository.GetFirstByConditionAsync(
            x => x.Id == request.Id,
            null,
            cancellationToken);
        if (overtimeRequest is null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.OvertimeRequestNotFound);

        var overlapping = await overtimeRequestRepository.GetQueryable()
            .Where(x => x.EmployeeId == overtimeRequest.EmployeeId
                && x.Id != request.Id
                && x.Status == OvertimeStatusCode.Approved
                && x.OvertimeDate == overtimeRequest.OvertimeDate
                && x.StartTime < overtimeRequest.EndTime
                && x.EndTime > overtimeRequest.StartTime)
            .AnyAsync(cancellationToken);
        if (overlapping)
            return HrErrorResponses.Create(HrBusinessErrorCodes.OverlappingOvertimeRequestsNotAllowed);

        overtimeRequest.Approve(request.ApprovedBy);

        var saveResult = await unitOfWork.SaveChangesAsync(cancellationToken);
        if (saveResult.IsT1)
        {
            return saveResult.AsT1 is DbUpdateConcurrencyException
                ? HrErrorResponses.Create(HrBusinessErrorCodes.OvertimeRequestConcurrencyConflict)
                : HrErrorResponses.Create(HrBusinessErrorCodes.SaveChangesFailed);
        }

        await publishEndpoint.Publish(new OvertimeRequestApprovedIntegrationEvent(
            overtimeRequest.Id.Value.ToString(),
            overtimeRequest.EmployeeId.Value.ToString()), cancellationToken);

        return new SuccessResponse();
    }
}
