using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.BuildingBlock.Application.Results;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Cqrs.Commands.LeaveRequestCommands.CancelLeaveRequest;
using OneOf;

namespace Anemoi.Hr.Application.Cqrs.Commands.LeaveRequestCommands.ForceCancelLeaveRequest;

public sealed class ForceCancelLeaveRequestHandler(CancelLeaveRequestHandler innerHandler)
    : ICommandHandler<ForceCancelLeaveRequestCommand, OneOf<None, ErrorDetailResponse>>
{
    public Task<OneOf<None, ErrorDetailResponse>> Handle(ForceCancelLeaveRequestCommand request,
        CancellationToken cancellationToken)
    {
        if (!request.SensitivePermissionConfirmed)
            return Task.FromResult<OneOf<None, ErrorDetailResponse>>(
                HrErrorResponses.Create(HrBusinessErrorCodes.PermissionSensitiveConfirmationRequired));

        return innerHandler.Handle(new CancelLeaveRequestCommand(request.Id, request.ActorEmployeeId, request.Reason),
            cancellationToken);
    }
}
