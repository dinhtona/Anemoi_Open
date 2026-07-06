using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.BuildingBlock.Application.Helpers;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.MasterData;
using Anemoi.Hr.ModelIds.ModelIds;
using OneOf;

namespace Anemoi.Hr.Application.Cqrs.Commands.LeavePolicyCommands.CreateLeavePolicy;

public sealed class CreateLeavePolicyHandler(
    ISqlRepository<LeavePolicy> repository,
    IUnitOfWork unitOfWork,
    MasterDataMapper mapper)
    : ICommandHandler<CreateLeavePolicyCommand, OneOf<LeavePolicyResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<LeavePolicyResponse, ErrorDetailResponse>> Handle(
        CreateLeavePolicyCommand request, CancellationToken cancellationToken)
    {
        var cleanCode = request.Code.Trim();
        var exists = await repository.ExistByConditionAsync(
            x => x.Code == cleanCode, cancellationToken);
        if (exists)
            return HrErrorResponses.Create(HrBusinessErrorCodes.LeavePolicySettingsCodeAlreadyExists);

        var duplicate = await repository.ExistByConditionAsync(
            x => x.LeaveTypeId == request.LeaveTypeId && x.ApplicableGradeCode == request.ApplicableGradeCode,
            cancellationToken);
        if (duplicate)
            return HrErrorResponses.Create(HrBusinessErrorCodes.LeavePolicySettingsDuplicate);

        var id = new LeavePolicyId(IdGenerator.NextGuid());
        var entity = LeavePolicy.Create(id, cleanCode, request.Name.Trim(),
            request.LeaveTypeId, request.ApplicableGradeCode, request.AnnualEntitlement);

        await repository.CreateOneAsync(entity, cancellationToken);
        var saveResult = await unitOfWork.SaveChangesAsync(cancellationToken);
        if (saveResult.IsT1)
            return HrErrorResponses.FromSaveResult(saveResult.AsT1, null);

        return mapper.ToLeavePolicyResponse(entity);
    }
}
