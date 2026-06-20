using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.BuildingBlock.Application.Helpers;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Overtime;
using Anemoi.Hr.ModelIds.ModelIds;
using OneOf;

namespace Anemoi.Hr.Application.Cqrs.Commands.OvertimeRuleCommands.CreateOvertimeRule;

public sealed class CreateOvertimeRuleHandler(
    ISqlRepository<OvertimeRule> repository,
    IUnitOfWork unitOfWork,
    MasterDataMapper mapper)
    : ICommandHandler<CreateOvertimeRuleCommand, OneOf<OvertimeRuleResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<OvertimeRuleResponse, ErrorDetailResponse>> Handle(
        CreateOvertimeRuleCommand request, CancellationToken cancellationToken)
    {
        var cleanCode = request.Code.Trim();
        var codeExists = await repository.ExistByConditionAsync(
            x => x.Code == cleanCode, cancellationToken);
        if (codeExists)
            return HrErrorResponses.Create(HrBusinessErrorCodes.OvertimeRuleCodeAlreadyExists);

        var id = new OvertimeRuleId(IdGenerator.NextGuid());
        var entity = OvertimeRule.Create(id, cleanCode, request.Name.Trim(),
            request.WeekdayMultiplier, request.WeekendMultiplier, request.HolidayMultiplier);

        await repository.CreateOneAsync(entity, cancellationToken);
        var saveResult = await unitOfWork.SaveChangesAsync(cancellationToken);
        if (saveResult.IsT1)
            return HrErrorResponses.FromSaveResult(saveResult.AsT1, null);

        return mapper.ToOvertimeRuleResponse(entity);
    }
}
