using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Positions;
using OneOf;

namespace Anemoi.Hr.Application.Cqrs.Commands.PositionCommands.ActivatePosition;

public sealed class ActivatePositionHandler(
    ISqlRepository<Position> positionRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<ActivatePositionCommand, OneOf<SuccessResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<SuccessResponse, ErrorDetailResponse>> Handle(
        ActivatePositionCommand request,
        CancellationToken cancellationToken)
    {
        var position = await positionRepository.GetFirstByConditionAsync(
            x => x.Id == request.Id,
            null,
            cancellationToken);

        if (position is null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.PositionNotFound);

        position.Activate();

        var saveResult = await unitOfWork.SaveChangesAsync(cancellationToken);

        if (saveResult.IsT1)
            return HrErrorResponses.FromSaveResult(saveResult.AsT1, null);

        return new SuccessResponse();
    }
}
