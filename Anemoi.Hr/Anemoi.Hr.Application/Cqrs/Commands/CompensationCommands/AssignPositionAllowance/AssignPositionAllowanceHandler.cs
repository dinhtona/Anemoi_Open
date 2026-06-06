using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.BuildingBlock.Application.Helpers;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Compensation;
using Anemoi.Hr.Domain.Positions;
using Anemoi.Hr.ModelIds.ModelIds;
using OneOf;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Anemoi.Hr.Application.Cqrs.Commands.CompensationCommands.AssignPositionAllowance;

public sealed class AssignPositionAllowanceHandler(
    ISqlRepository<Position> positionRepository,
    ISqlRepository<AllowanceType> allowanceTypeRepository,
    ISqlRepository<PositionAllowance> positionAllowanceRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<AssignPositionAllowanceCommand, OneOf<AssignPositionAllowanceResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<AssignPositionAllowanceResponse, ErrorDetailResponse>> Handle(
        AssignPositionAllowanceCommand request,
        CancellationToken cancellationToken)
    {
        var positionExists = await positionRepository.ExistByConditionAsync(
            x => x.Id == request.PositionId,
            cancellationToken);
        if (!positionExists)
            return HrErrorResponses.Create(HrBusinessErrorCodes.PositionNotFound);

        var typeExists = await allowanceTypeRepository.ExistByConditionAsync(
            x => x.Id == request.AllowanceTypeId && x.IsActive,
            cancellationToken);
        if (!typeExists)
            return HrErrorResponses.Create("HR_ALLOWANCE_TYPE_NOT_FOUND");

        var exists = await positionAllowanceRepository.ExistByConditionAsync(
            x => x.PositionId == request.PositionId && x.AllowanceTypeId == request.AllowanceTypeId,
            cancellationToken);
        if (exists)
            return HrErrorResponses.Create("HR_POSITION_ALLOWANCE_ALREADY_EXISTS");

        var newAllowance = new PositionAllowance
        {
            Id = new PositionAllowanceId(IdGenerator.NextGuid()),
            PositionId = request.PositionId,
            AllowanceTypeId = request.AllowanceTypeId,
            Amount = request.Amount,
            Currency = request.Currency.Trim(),
            IsActive = request.IsActive,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await positionAllowanceRepository.CreateOneAsync(newAllowance, cancellationToken);
        var saveResult = await unitOfWork.SaveChangesAsync(cancellationToken);
        
        if (saveResult.IsT1)
            return HrErrorResponses.Create("HR_SAVE_CHANGES_FAILED");

        return new AssignPositionAllowanceResponse { PositionAllowanceId = newAllowance.Id.Value.ToString() };
    }
}
