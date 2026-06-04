using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.BuildingBlock.Application.Results;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Domain.Leaves;
using OneOf;

namespace Anemoi.Hr.Application.Cqrs.Commands.LeavePolicyCommands.UpdateLeavePolicy;

public sealed class UpdateLeavePolicyHandler(ISqlRepository<LeavePolicy> leavePolicyRepository, IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateLeavePolicyCommand, OneOf<None, ErrorDetailResponse>>
{
    public async Task<OneOf<None, ErrorDetailResponse>> Handle(UpdateLeavePolicyCommand request,
        CancellationToken cancellationToken)
    {
        var policy = await leavePolicyRepository.GetFirstByConditionAsync(x => x.Id == request.Id, null, cancellationToken);
        if (policy is null) return HrErrorResponses.Create(HrBusinessErrorCodes.LeavePolicyNotFound);

        policy.Name = request.Name;
        policy.MonthlyAccrualDays = request.MonthlyAccrualDays;
        policy.AnnualMaxDays = request.AnnualMaxDays;
        policy.AllowCarryForward = request.AllowCarryForward;
        policy.MaxCarryForwardDays = request.MaxCarryForwardDays;
        policy.IsActive = request.IsActive;
        policy.UpdatedAt = DateTime.UtcNow;

        var saveResult = await unitOfWork.SaveChangesAsync(cancellationToken);
        return saveResult.Match<OneOf<None, ErrorDetailResponse>>(
            _ => None.Value,
            _ => HrErrorResponses.Create("HR_SAVE_CHANGES_FAILED"));
    }
}
