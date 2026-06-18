using Anemoi.BuildingBlock.Application.Helpers;
using Anemoi.Hr.Application.Cqrs.Commands.LeavePolicyCommands.CreateLeavePolicy;
using Anemoi.Hr.Application.Cqrs.Commands.LeaveRequestCommands.SubmitLeaveRequest;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Leaves;
using Anemoi.Hr.ModelIds.ModelIds;
using Riok.Mapperly.Abstractions;

namespace Anemoi.Hr.Application.Mappings;

[Mapper]
public partial class LeaveMapper
{
    public LeavePolicy ToLeavePolicy(CreateLeavePolicyCommand command)
    {
        return new LeavePolicy
        {
            Id = new LeavePolicyId(IdGenerator.NextGuid()),
            Code = command.Code,
            Name = command.Name,
            LeaveTypeCode = command.LeaveTypeCode,
            MonthlyAccrualDays = command.MonthlyAccrualDays,
            AnnualMaxDays = command.AnnualMaxDays,
            AllowCarryForward = command.AllowCarryForward,
            MaxCarryForwardDays = command.MaxCarryForwardDays,
            IsActive = command.IsActive,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public LeaveRequest ToLeaveRequest(SubmitLeaveRequestCommand command)
    {
        return new LeaveRequest
        {
            Id = new LeaveRequestId(IdGenerator.NextGuid()),
            EmployeeId = command.EmployeeId,
            LeavePolicyId = command.LeavePolicyId,
            ApproverEmployeeId = new EmployeeId(Guid.Empty),
            LeaveTypeCode = command.LeaveTypeCode,
            StartDate = command.StartDate,
            EndDate = command.EndDate,
            RequestedDays = command.RequestedDays,
            StatusCode = LeaveRequestStatusCode.Pending,
            Reason = command.Reason,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public LeavePolicyIdResponse ToLeavePolicyIdResponse(LeavePolicy policy) => new(policy.Id.Value.ToString());

    public LeaveRequestIdResponse ToLeaveRequestIdResponse(LeaveRequest request) => new(request.Id.Value.ToString());

    public LeavePolicyResponse ToLeavePolicyResponse(LeavePolicy policy)
    {
        return new LeavePolicyResponse
        {
            Id = policy.Id.Value.ToString(),
            Code = policy.Code,
            Name = policy.Name,
            LeaveTypeCode = policy.LeaveTypeCode,
            MonthlyAccrualDays = policy.MonthlyAccrualDays,
            AnnualMaxDays = policy.AnnualMaxDays,
            AllowCarryForward = policy.AllowCarryForward,
            MaxCarryForwardDays = policy.MaxCarryForwardDays,
            IsActive = policy.IsActive,
            CreatedAt = policy.CreatedAt,
            UpdatedAt = policy.UpdatedAt
        };
    }

    public LeaveBalanceResponse ToLeaveBalanceResponse(LeaveBalance balance)
    {
        return new LeaveBalanceResponse
        {
            Id = balance.Id.Value.ToString(),
            EmployeeId = balance.EmployeeId.Value.ToString(),
            LeavePolicyId = balance.LeavePolicyId.Value.ToString(),
            Year = balance.Year,
            OpeningDays = balance.OpeningDays,
            AccruedDays = balance.AccruedDays,
            UsedDays = balance.UsedDays,
            PendingDays = balance.PendingDays,
            AdjustedDays = balance.AdjustedDays,
            RemainingDays = balance.RemainingDays,
            CreatedAt = balance.CreatedAt,
            UpdatedAt = balance.UpdatedAt
        };
    }

    public LeaveRequestResponse ToLeaveRequestResponse(LeaveRequest request)
    {
        return new LeaveRequestResponse
        {
            Id = request.Id.Value.ToString(),
            EmployeeId = request.EmployeeId.Value.ToString(),
            LeavePolicyId = request.LeavePolicyId.Value.ToString(),
            ApproverEmployeeId = request.ApproverEmployeeId?.Value.ToString(),
            ApproverName = request.ApproverEmployee?.FullName,
            LeaveTypeCode = request.LeaveTypeCode,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            RequestedDays = request.RequestedDays,
            StatusCode = request.StatusCode,
            Reason = request.Reason,
            CreatedAt = request.CreatedAt,
            UpdatedAt = request.UpdatedAt
        };
    }

    public LeaveTransactionResponse ToLeaveTransactionResponse(LeaveTransaction transaction)
    {
        return new LeaveTransactionResponse
        {
            Id = transaction.Id.Value.ToString(),
            EmployeeId = transaction.EmployeeId.Value.ToString(),
            LeavePolicyId = transaction.LeavePolicyId.Value.ToString(),
            LeaveBalanceId = transaction.LeaveBalanceId.Value.ToString(),
            LeaveRequestId = transaction.LeaveRequestId?.Value.ToString(),
            TransactionTypeCode = transaction.TransactionTypeCode,
            Days = transaction.Days,
            BalanceAfterDays = transaction.BalanceAfterDays,
            SourceType = transaction.SourceType,
            SourceId = transaction.SourceId,
            Reason = transaction.Reason,
            CreatedAt = transaction.CreatedAt
        };
    }

    public LeaveAccrualRunResponse ToLeaveAccrualRunResponse(LeaveAccrualRun run)
    {
        return new LeaveAccrualRunResponse
        {
            Id = run.Id.Value.ToString(),
            EmployeeId = run.EmployeeId.Value.ToString(),
            LeavePolicyId = run.LeavePolicyId.Value.ToString(),
            YearMonth = run.YearMonth,
            AccruedDays = run.AccruedDays,
            CreatedAt = run.CreatedAt
        };
    }
}
