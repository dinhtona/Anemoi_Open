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

namespace Anemoi.Hr.Application.Cqrs.Commands.CompensationCommands.UpdateAllowanceType;

public sealed class UpdateAllowanceTypeHandler(
    ISqlRepository<AllowanceType> allowanceTypeRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateAllowanceTypeCommand, OneOf<UpdateAllowanceTypeResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<UpdateAllowanceTypeResponse, ErrorDetailResponse>> Handle(
        UpdateAllowanceTypeCommand request,
        CancellationToken cancellationToken)
    {
        var allowanceType = await allowanceTypeRepository.GetFirstByConditionAsync(
            x => x.Id == request.AllowanceTypeId,
            null,
            cancellationToken);

        if (allowanceType is null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.AllowanceTypeNotFound);

        allowanceType.UpdateDetails(
            request.Name,
            request.Description,
            request.IsTaxable,
            false,
            request.IsActive);

        var saveResult = await unitOfWork.SaveChangesAsync(cancellationToken);

        if (saveResult.IsT1)
            return HrErrorResponses.FromSaveResult(saveResult.AsT1, HrBusinessErrorCodes.AllowanceTypeConcurrencyConflict);

        return new UpdateAllowanceTypeResponse { AllowanceTypeId = allowanceType.Id.Value.ToString() };
    }
}
