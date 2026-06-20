using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.BuildingBlock.Application.Helpers;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Positions;
using Anemoi.Hr.ModelIds.ModelIds;
using OneOf;

namespace Anemoi.Hr.Application.Cqrs.Commands.PositionCommands.CreatePosition;

public sealed class CreatePositionHandler(
    ISqlRepository<Position> positionRepository,
    IUnitOfWork unitOfWork,
    EmployeeMapper mapper)
    : ICommandHandler<CreatePositionCommand, OneOf<PositionResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<PositionResponse, ErrorDetailResponse>> Handle(
        CreatePositionCommand request,
        CancellationToken cancellationToken)
    {
        if (!PositionTypeCode.IsValid(request.PositionTypeCode))
            return HrErrorResponses.Create(HrBusinessErrorCodes.ValPositionTypeCodeInvalid);

        var cleanCode = request.Code.Trim();
        var codeExists = await positionRepository.ExistByConditionAsync(
            x => x.Code == cleanCode,
            cancellationToken);

        if (codeExists)
            return HrErrorResponses.Create(HrBusinessErrorCodes.PositionCodeExists);

        var id = new PositionId(IdGenerator.NextGuid());
        var position = Position.Create(
            id,
            request.DepartmentId,
            cleanCode,
            request.Name.Trim(),
            request.PositionTypeCode);

        await positionRepository.CreateOneAsync(position, cancellationToken);
        var saveResult = await unitOfWork.SaveChangesAsync(cancellationToken);

        if (saveResult.IsT1)
            return HrErrorResponses.FromSaveResult(saveResult.AsT1, null);

        return mapper.ToPositionResponse(position);
    }
}
