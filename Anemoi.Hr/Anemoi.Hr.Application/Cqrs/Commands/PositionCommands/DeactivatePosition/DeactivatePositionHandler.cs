using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Employees;
using Anemoi.Hr.Domain.Positions;
using OneOf;

namespace Anemoi.Hr.Application.Cqrs.Commands.PositionCommands.DeactivatePosition;

public sealed class DeactivatePositionHandler(
    ISqlRepository<Position> positionRepository,
    ISqlRepository<Employee> employeeRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<DeactivatePositionCommand, OneOf<SuccessResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<SuccessResponse, ErrorDetailResponse>> Handle(
        DeactivatePositionCommand request,
        CancellationToken cancellationToken)
    {
        var position = await positionRepository.GetFirstByConditionAsync(
            x => x.Id == request.Id,
            null,
            cancellationToken);

        if (position is null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.PositionNotFound);

        var hasActiveEmployees = await employeeRepository.ExistByConditionAsync(
            x => x.PrimaryPositionId == request.Id,
            cancellationToken);

        if (hasActiveEmployees)
            return HrErrorResponses.Create(HrBusinessErrorCodes.PositionHasActiveEmployees);

        position.Deactivate();

        var saveResult = await unitOfWork.SaveChangesAsync(cancellationToken);

        if (saveResult.IsT1)
            return HrErrorResponses.FromSaveResult(saveResult.AsT1, null);

        return new SuccessResponse();
    }
}
