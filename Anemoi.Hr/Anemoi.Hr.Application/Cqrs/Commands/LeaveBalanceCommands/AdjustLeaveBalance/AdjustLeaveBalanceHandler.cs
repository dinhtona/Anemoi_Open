using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Microsoft.EntityFrameworkCore;
using Anemoi.BuildingBlock.Application.Helpers;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.BuildingBlock.Application.Results;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Events;
using Anemoi.Hr.Domain.Leaves;
using Anemoi.Hr.ModelIds.ModelIds;
using MassTransit;
using OneOf;

namespace Anemoi.Hr.Application.Cqrs.Commands.LeaveBalanceCommands.AdjustLeaveBalance;

public sealed class AdjustLeaveBalanceHandler(
    ISqlRepository<LeaveBalance> leaveBalanceRepository,
    ISqlRepository<LeaveTransaction> leaveTransactionRepository,
    IUnitOfWork unitOfWork,
    IPublishEndpoint publishEndpoint)
    : ICommandHandler<AdjustLeaveBalanceCommand, OneOf<None, ErrorDetailResponse>>
{
    public async Task<OneOf<None, ErrorDetailResponse>> Handle(AdjustLeaveBalanceCommand request,
        CancellationToken cancellationToken)
    {
        if (!request.SensitivePermissionConfirmed)
            return HrErrorResponses.Create(HrBusinessErrorCodes.PermissionSensitiveConfirmationRequired);

        var balance = await leaveBalanceRepository.GetFirstByConditionAsync(
            x => x.EmployeeId == request.EmployeeId && x.LeavePolicyId == request.LeavePolicyId && x.Year == request.Year,
            null,
            cancellationToken);
        if (balance is null) return HrErrorResponses.Create(HrBusinessErrorCodes.LeaveBalanceNotFound);

        balance.AdjustedDays += request.Days;
        balance.RemainingDays += request.Days;
        balance.UpdatedAt = DateTime.UtcNow;
        await leaveTransactionRepository.CreateOneAsync(new LeaveTransaction
        {
            Id = new LeaveTransactionId(IdGenerator.NextGuid()),
            EmployeeId = request.EmployeeId,
            LeavePolicyId = request.LeavePolicyId,
            LeaveBalanceId = balance.Id,
            TransactionTypeCode = "Adjustment",
            Days = request.Days,
            BalanceAfterDays = balance.RemainingDays,
            SourceType = request.SourceType,
            SourceId = request.SourceId,
            Reason = request.Reason,
            CreatedAt = DateTime.UtcNow
        }, cancellationToken);

        var saveResult = await unitOfWork.SaveChangesAsync(cancellationToken);
        if (saveResult.IsT1)
        {
            return saveResult.AsT1 is DbUpdateConcurrencyException
                ? HrErrorResponses.Create(HrBusinessErrorCodes.LeaveBalanceConcurrencyConflict)
                : HrErrorResponses.Create("HR_SAVE_CHANGES_FAILED");
        }

        await publishEndpoint.Publish(new LeaveBalanceChangedIntegrationEvent(
            balance.EmployeeId.Value.ToString(),
            balance.LeavePolicyId.Value.ToString(),
            balance.Year,
            balance.RemainingDays,
            "Adjustment"), cancellationToken);

        return None.Value;
    }
}
