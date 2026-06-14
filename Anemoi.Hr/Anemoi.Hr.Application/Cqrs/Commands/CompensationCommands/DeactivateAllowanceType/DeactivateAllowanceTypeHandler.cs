using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Compensation;
using Microsoft.EntityFrameworkCore;
using OneOf;
using System.Threading;
using System.Threading.Tasks;

namespace Anemoi.Hr.Application.Cqrs.Commands.CompensationCommands.DeactivateAllowanceType;

public sealed class DeactivateAllowanceTypeHandler(
    ISqlRepository<AllowanceType> allowanceTypeRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<DeactivateAllowanceTypeCommand, OneOf<DeactivateAllowanceTypeResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<DeactivateAllowanceTypeResponse, ErrorDetailResponse>> Handle(
        DeactivateAllowanceTypeCommand request,
        CancellationToken cancellationToken)
    {
        var allowanceType = await allowanceTypeRepository.GetFirstByConditionAsync(
            x => x.Id == request.AllowanceTypeId,
            null,
            cancellationToken);

        if (allowanceType is null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.AllowanceTypeNotFound);

        if (!allowanceType.IsActive)
            return new DeactivateAllowanceTypeResponse { AllowanceTypeId = allowanceType.Id.Value.ToString() };

        allowanceType.Deactivate();

        var saveResult = await unitOfWork.SaveChangesAsync(cancellationToken);

        if (saveResult.IsT1)
        {
            return saveResult.AsT1 is DbUpdateConcurrencyException
                ? HrErrorResponses.Create(HrBusinessErrorCodes.AllowanceTypeConcurrencyConflict)
                : HrErrorResponses.Create(HrBusinessErrorCodes.SaveChangesFailed);
        }

        return new DeactivateAllowanceTypeResponse { AllowanceTypeId = allowanceType.Id.Value.ToString() };
    }
}
