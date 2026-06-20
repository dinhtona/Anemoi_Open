using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Compensation;
using OneOf;

namespace Anemoi.Hr.Application.Cqrs.Commands.CompensationCommands.ActivateAllowanceType;

public sealed class ActivateAllowanceTypeHandler(
    ISqlRepository<AllowanceType> repository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<ActivateAllowanceTypeCommand, OneOf<SuccessResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<SuccessResponse, ErrorDetailResponse>> Handle(
        ActivateAllowanceTypeCommand request, CancellationToken cancellationToken)
    {
        var entity = await repository.GetFirstByConditionAsync(
            x => x.Id == request.Id, null, cancellationToken);
        if (entity is null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.AllowanceTypeNotFound);

        entity.Activate();
        var saveResult = await unitOfWork.SaveChangesAsync(cancellationToken);
        if (saveResult.IsT1)
            return HrErrorResponses.FromSaveResult(saveResult.AsT1, null);
        return new SuccessResponse();
    }
}
