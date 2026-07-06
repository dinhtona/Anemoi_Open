using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Overtime;
using OneOf;

namespace Anemoi.Hr.Application.Cqrs.Commands.OvertimeRuleCommands.UpdateOvertimeRule;

public sealed class UpdateOvertimeRuleHandler(
    ISqlRepository<OvertimeRule> repository,
    IUnitOfWork unitOfWork,
    MasterDataMapper mapper)
    : ICommandHandler<UpdateOvertimeRuleCommand, OneOf<OvertimeRuleResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<OvertimeRuleResponse, ErrorDetailResponse>> Handle(
        UpdateOvertimeRuleCommand request, CancellationToken cancellationToken)
    {
        var entity = await repository.GetFirstByConditionAsync(
            x => x.Id == request.Id, null, cancellationToken);
        if (entity is null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.OvertimeRuleNotFound);

        var cleanCode = request.Code.Trim();
        var codeExists = await repository.ExistByConditionAsync(
            x => x.Code == cleanCode && x.Id != request.Id, cancellationToken);
        if (codeExists)
            return HrErrorResponses.Create(HrBusinessErrorCodes.OvertimeRuleCodeAlreadyExists);

        entity.UpdateInfo(cleanCode, request.Name.Trim(),
            request.WeekdayMultiplier, request.WeekendMultiplier, request.HolidayMultiplier);

        var saveResult = await unitOfWork.SaveChangesAsync(cancellationToken);
        if (saveResult.IsT1)
            return HrErrorResponses.FromSaveResult(saveResult.AsT1, null);

        return mapper.ToOvertimeRuleResponse(entity);
    }
}
