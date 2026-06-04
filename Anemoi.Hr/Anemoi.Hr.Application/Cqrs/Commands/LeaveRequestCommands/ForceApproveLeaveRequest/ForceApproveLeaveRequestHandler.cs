using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.BuildingBlock.Application.Results;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Cqrs.Commands.LeaveRequestCommands.ApproveLeaveRequest;
using OneOf;

namespace Anemoi.Hr.Application.Cqrs.Commands.LeaveRequestCommands.ForceApproveLeaveRequest;

public sealed class ForceApproveLeaveRequestHandler(ApproveLeaveRequestHandler innerHandler)
    : ICommandHandler<ForceApproveLeaveRequestCommand, OneOf<None, ErrorDetailResponse>>
{
    public Task<OneOf<None, ErrorDetailResponse>> Handle(ForceApproveLeaveRequestCommand request,
        CancellationToken cancellationToken)
    {
        if (!request.SensitivePermissionConfirmed)
            return Task.FromResult<OneOf<None, ErrorDetailResponse>>(
                HrErrorResponses.Create(HrBusinessErrorCodes.PermissionSensitiveConfirmationRequired));

        return innerHandler.Handle(new ApproveLeaveRequestCommand(request.Id, request.ActorEmployeeId, request.Reason),
            cancellationToken);
    }
}
