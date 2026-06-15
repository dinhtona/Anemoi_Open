using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Microsoft.EntityFrameworkCore;
using Anemoi.BuildingBlock.Application.Helpers;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Events;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Leaves;
using Anemoi.Hr.ModelIds.ModelIds;
using MassTransit;
using OneOf;

namespace Anemoi.Hr.Application.Cqrs.Commands.LeaveRequestCommands.SubmitLeaveRequest;

public sealed class SubmitLeaveRequestHandler(
    ISqlRepository<LeavePolicy> leavePolicyRepository,
    ISqlRepository<LeaveBalance> leaveBalanceRepository,
    ISqlRepository<LeaveRequest> leaveRequestRepository,
    ISqlRepository<LeaveTransaction> leaveTransactionRepository,
    IUnitOfWork unitOfWork,
    IPublishEndpoint publishEndpoint,
    LeaveMapper mapper)
    : ICommandHandler<SubmitLeaveRequestCommand, OneOf<LeaveRequestIdResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<LeaveRequestIdResponse, ErrorDetailResponse>> Handle(SubmitLeaveRequestCommand request,
        CancellationToken cancellationToken)
    {
        var policy = await leavePolicyRepository.GetFirstByConditionAsync(x => x.Id == request.LeavePolicyId,
            null, cancellationToken);
        if (policy is null) return HrErrorResponses.Create(HrBusinessErrorCodes.LeavePolicyNotFound);

        var year = request.StartDate.Year;
        var balance = await leaveBalanceRepository.GetFirstByConditionAsync(
            x => x.EmployeeId == request.EmployeeId && x.LeavePolicyId == request.LeavePolicyId && x.Year == year,
            null,
            cancellationToken);
        if (balance is null) return HrErrorResponses.Create(HrBusinessErrorCodes.LeaveBalanceNotFound);
        if (balance.RemainingDays < request.RequestedDays)
            return HrErrorResponses.Create(HrBusinessErrorCodes.LeaveBalanceNotEnough);

        var isOverlapping = await leaveRequestRepository.ExistByConditionAsync(
            x => x.EmployeeId == request.EmployeeId
                 && (x.StatusCode == "Pending" || x.StatusCode == "Approved")
                 && x.StartDate <= request.EndDate
                 && request.StartDate <= x.EndDate,
            cancellationToken);
        if (isOverlapping)
            return HrErrorResponses.Create(HrBusinessErrorCodes.LeaveRequestOverlapping);

        var leaveRequest = mapper.ToLeaveRequest(request);
        balance.PendingDays += request.RequestedDays;
        balance.RemainingDays -= request.RequestedDays;
        balance.UpdatedAt = DateTime.UtcNow;

        await leaveRequestRepository.CreateOneAsync(leaveRequest, cancellationToken);
        await leaveTransactionRepository.CreateOneAsync(new LeaveTransaction
        {
            Id = new LeaveTransactionId(IdGenerator.NextGuid()),
            EmployeeId = request.EmployeeId,
            LeavePolicyId = request.LeavePolicyId,
            LeaveBalanceId = balance.Id,
            LeaveRequestId = leaveRequest.Id,
            TransactionTypeCode = "PendingReserve",
            Days = request.RequestedDays,
            BalanceAfterDays = balance.RemainingDays,
            SourceType = "LeaveRequest",
            SourceId = leaveRequest.Id.Value.ToString(),
            Reason = request.Reason,
            CreatedAt = DateTime.UtcNow
        }, cancellationToken);

        var saveResult = await unitOfWork.SaveChangesAsync(cancellationToken);
        if (saveResult.IsT1)
        {
            return saveResult.AsT1 is DbUpdateConcurrencyException
                ? HrErrorResponses.Create(HrBusinessErrorCodes.LeaveBalanceConcurrencyConflict)
                : HrErrorResponses.Create(HrBusinessErrorCodes.SaveChangesFailed);
        }

        await publishEndpoint.Publish(new LeaveRequestSubmittedIntegrationEvent(
            leaveRequest.Id.Value.ToString(),
            leaveRequest.EmployeeId.Value.ToString(),
            leaveRequest.LeavePolicyId.Value.ToString()), cancellationToken);
        await publishEndpoint.Publish(new LeaveBalanceChangedIntegrationEvent(
            balance.EmployeeId.Value.ToString(),
            balance.LeavePolicyId.Value.ToString(),
            balance.Year,
            balance.RemainingDays,
            "PendingReserve"), cancellationToken);

        return mapper.ToLeaveRequestIdResponse(leaveRequest);
    }
}
