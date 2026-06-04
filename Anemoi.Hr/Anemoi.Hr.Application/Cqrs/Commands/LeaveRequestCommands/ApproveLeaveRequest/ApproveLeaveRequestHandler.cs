using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.BuildingBlock.Application.Helpers;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.BuildingBlock.Application.Results;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Events;
using Anemoi.Hr.Domain.Leaves;
using Anemoi.Hr.ModelIds.ModelIds;
using MassTransit;
using OneOf;

namespace Anemoi.Hr.Application.Cqrs.Commands.LeaveRequestCommands.ApproveLeaveRequest;

public sealed class ApproveLeaveRequestHandler(
    ISqlRepository<LeaveRequest> leaveRequestRepository,
    ISqlRepository<LeaveBalance> leaveBalanceRepository,
    ISqlRepository<LeaveTransaction> leaveTransactionRepository,
    IUnitOfWork unitOfWork,
    IPublishEndpoint publishEndpoint)
    : ICommandHandler<ApproveLeaveRequestCommand, OneOf<None, ErrorDetailResponse>>
{
    public async Task<OneOf<None, ErrorDetailResponse>> Handle(ApproveLeaveRequestCommand request,
        CancellationToken cancellationToken)
    {
        var leaveRequest = await leaveRequestRepository.GetFirstByConditionAsync(x => x.Id == request.Id,
            null, cancellationToken);
        if (leaveRequest is null) return HrErrorResponses.Create(HrBusinessErrorCodes.LeaveRequestNotFound);
        if (leaveRequest.StatusCode == "Approved")
            return HrErrorResponses.Create(HrBusinessErrorCodes.LeaveRequestAlreadyApproved);
        if (leaveRequest.StatusCode is "Rejected" or "Cancelled")
            return HrErrorResponses.Create(HrBusinessErrorCodes.LeaveRequestInvalidStatus);

        var balance = await leaveBalanceRepository.GetFirstByConditionAsync(
            x => x.EmployeeId == leaveRequest.EmployeeId &&
                x.LeavePolicyId == leaveRequest.LeavePolicyId &&
                x.Year == leaveRequest.StartDate.Year,
            null,
            cancellationToken);
        if (balance is null) return HrErrorResponses.Create(HrBusinessErrorCodes.LeaveBalanceNotFound);

        balance.PendingDays -= leaveRequest.RequestedDays;
        balance.UsedDays += leaveRequest.RequestedDays;
        balance.UpdatedAt = DateTime.UtcNow;
        leaveRequest.StatusCode = "Approved";
        leaveRequest.UpdatedAt = DateTime.UtcNow;

        await leaveTransactionRepository.CreateManyAsync([
            NewTransaction(leaveRequest, balance, "PendingRelease", leaveRequest.RequestedDays, request.Comment),
            NewTransaction(leaveRequest, balance, "Used", leaveRequest.RequestedDays, request.Comment)
        ], cancellationToken);

        var saveResult = await unitOfWork.SaveChangesAsync(cancellationToken);
        if (saveResult.IsT1) return HrErrorResponses.Create("HR_SAVE_CHANGES_FAILED");

        await publishEndpoint.Publish(new LeaveRequestApprovedIntegrationEvent(
            leaveRequest.Id.Value.ToString(),
            leaveRequest.EmployeeId.Value.ToString(),
            leaveRequest.LeavePolicyId.Value.ToString()), cancellationToken);
        await publishEndpoint.Publish(new LeaveBalanceChangedIntegrationEvent(
            balance.EmployeeId.Value.ToString(),
            balance.LeavePolicyId.Value.ToString(),
            balance.Year,
            balance.RemainingDays,
            "Used"), cancellationToken);

        return None.Value;
    }

    private static LeaveTransaction NewTransaction(LeaveRequest request, LeaveBalance balance, string type, decimal days,
        string reason)
    {
        return new LeaveTransaction
        {
            Id = new LeaveTransactionId(IdGenerator.NextGuid()),
            EmployeeId = request.EmployeeId,
            LeavePolicyId = request.LeavePolicyId,
            LeaveBalanceId = balance.Id,
            LeaveRequestId = request.Id,
            TransactionTypeCode = type,
            Days = days,
            BalanceAfterDays = balance.RemainingDays,
            SourceType = "LeaveRequest",
            SourceId = request.Id.Value.ToString(),
            Reason = reason,
            CreatedAt = DateTime.UtcNow
        };
    }
}
