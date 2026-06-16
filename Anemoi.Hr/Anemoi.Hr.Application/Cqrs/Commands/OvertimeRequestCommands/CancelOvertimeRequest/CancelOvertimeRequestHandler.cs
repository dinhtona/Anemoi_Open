using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Microsoft.EntityFrameworkCore;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Contract.Hr.Events;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Overtime;
using MassTransit;
using OneOf;

namespace Anemoi.Hr.Application.Cqrs.Commands.OvertimeRequestCommands.CancelOvertimeRequest;

public sealed class CancelOvertimeRequestHandler(
    ISqlRepository<OvertimeRequest> overtimeRequestRepository,
    IUnitOfWork unitOfWork,
    IPublishEndpoint publishEndpoint)
    : ICommandHandler<CancelOvertimeRequestCommand, OneOf<SuccessResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<SuccessResponse, ErrorDetailResponse>> Handle(CancelOvertimeRequestCommand request,
        CancellationToken cancellationToken)
    {
        var overtimeRequest = await overtimeRequestRepository.GetFirstByConditionAsync(
            x => x.Id == request.Id,
            null,
            cancellationToken);
        if (overtimeRequest is null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.OvertimeRequestNotFound);

        overtimeRequest.Cancel(request.CancelledBy);

        await publishEndpoint.Publish(new OvertimeRequestCancelledIntegrationEvent(
            overtimeRequest.Id.Value.ToString(),
            overtimeRequest.EmployeeId.Value.ToString()), cancellationToken);

        var saveResult = await unitOfWork.SaveChangesAsync(cancellationToken);
        if (saveResult.IsT1)
        {
            return saveResult.AsT1 is DbUpdateConcurrencyException
                ? HrErrorResponses.Create(HrBusinessErrorCodes.OvertimeRequestConcurrencyConflict)
                : HrErrorResponses.Create(HrBusinessErrorCodes.SaveChangesFailed);
        }

        return new SuccessResponse();
    }
}
