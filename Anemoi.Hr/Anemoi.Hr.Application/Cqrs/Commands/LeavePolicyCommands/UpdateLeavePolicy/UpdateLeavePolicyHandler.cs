using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.MasterData;
using OneOf;

namespace Anemoi.Hr.Application.Cqrs.Commands.LeavePolicyCommands.UpdateLeavePolicy;

public sealed class UpdateLeavePolicyHandler(
    ISqlRepository<LeavePolicy> repository,
    IUnitOfWork unitOfWork,
    MasterDataMapper mapper)
    : ICommandHandler<UpdateLeavePolicyCommand, OneOf<LeavePolicyResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<LeavePolicyResponse, ErrorDetailResponse>> Handle(
        UpdateLeavePolicyCommand request, CancellationToken cancellationToken)
    {
        var entity = await repository.GetFirstByConditionAsync(
            x => x.Id == request.Id, null, cancellationToken);
        if (entity is null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.LeavePolicySettingsNotFound);

        var cleanCode = request.Code.Trim();
        var codeExists = await repository.ExistByConditionAsync(
            x => x.Code == cleanCode && x.Id != request.Id, cancellationToken);
        if (codeExists)
            return HrErrorResponses.Create(HrBusinessErrorCodes.LeavePolicySettingsCodeAlreadyExists);

        var duplicate = await repository.ExistByConditionAsync(
            x => x.LeaveTypeId == request.LeaveTypeId && x.ApplicableGradeCode == request.ApplicableGradeCode && x.Id != request.Id,
            cancellationToken);
        if (duplicate)
            return HrErrorResponses.Create(HrBusinessErrorCodes.LeavePolicySettingsDuplicate);

        entity.UpdateInfo(cleanCode, request.Name.Trim(), request.LeaveTypeId, request.ApplicableGradeCode, request.AnnualEntitlement);

        var saveResult = await unitOfWork.SaveChangesAsync(cancellationToken);
        if (saveResult.IsT1)
            return HrErrorResponses.FromSaveResult(saveResult.AsT1, null);

        return mapper.ToLeavePolicyResponse(entity);
    }
}
