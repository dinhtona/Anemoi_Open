using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Leaves;
using OneOf;

namespace Anemoi.Hr.Application.Cqrs.Commands.LeavePolicyCommands.CreateLeavePolicy;

public sealed class CreateLeavePolicyHandler(
    ISqlRepository<LeavePolicy> leavePolicyRepository,
    IUnitOfWork unitOfWork,
    LeaveMapper mapper)
    : ICommandHandler<CreateLeavePolicyCommand, OneOf<LeavePolicyIdResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<LeavePolicyIdResponse, ErrorDetailResponse>> Handle(CreateLeavePolicyCommand request,
        CancellationToken cancellationToken)
    {
        var exists = await leavePolicyRepository.ExistByConditionAsync(x => x.Code == request.Code, cancellationToken);
        if (exists) return HrErrorResponses.Create(HrBusinessErrorCodes.LeavePolicyCodeAlreadyExists);

        var policy = mapper.ToLeavePolicy(request);
        await leavePolicyRepository.CreateOneAsync(policy, cancellationToken);
        var saveResult = await unitOfWork.SaveChangesAsync(cancellationToken);
        return saveResult.Match<OneOf<LeavePolicyIdResponse, ErrorDetailResponse>>(
            _ => mapper.ToLeavePolicyIdResponse(policy),
            _ => HrErrorResponses.Create("HR_SAVE_CHANGES_FAILED"));
    }
}
