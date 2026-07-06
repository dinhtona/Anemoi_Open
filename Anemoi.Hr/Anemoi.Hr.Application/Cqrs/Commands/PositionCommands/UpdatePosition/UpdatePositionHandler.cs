using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Positions;
using OneOf;

namespace Anemoi.Hr.Application.Cqrs.Commands.PositionCommands.UpdatePosition;

public sealed class UpdatePositionHandler(
    ISqlRepository<Position> positionRepository,
    IUnitOfWork unitOfWork,
    EmployeeMapper mapper)
    : ICommandHandler<UpdatePositionCommand, OneOf<PositionResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<PositionResponse, ErrorDetailResponse>> Handle(
        UpdatePositionCommand request,
        CancellationToken cancellationToken)
    {
        var position = await positionRepository.GetFirstByConditionAsync(
            x => x.Id == request.Id,
            null,
            cancellationToken);

        if (position is null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.PositionNotFound);

        var cleanCode = request.Code.Trim();
        var codeExists = await positionRepository.ExistByConditionAsync(
            x => x.Code == cleanCode && x.Id != request.Id,
            cancellationToken);

        if (codeExists)
            return HrErrorResponses.Create(HrBusinessErrorCodes.PositionCodeExists);

        position.UpdateInfo(
            cleanCode,
            request.Name.Trim(),
            request.PositionTypeCode,
            request.DepartmentId);

        var saveResult = await unitOfWork.SaveChangesAsync(cancellationToken);

        if (saveResult.IsT1)
            return HrErrorResponses.FromSaveResult(saveResult.AsT1, null);

        return mapper.ToPositionResponse(position);
    }
}
