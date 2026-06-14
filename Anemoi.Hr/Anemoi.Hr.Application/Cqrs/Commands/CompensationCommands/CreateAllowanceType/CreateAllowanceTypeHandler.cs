using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.BuildingBlock.Application.Helpers;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Compensation;
using Anemoi.Hr.ModelIds.ModelIds;
using OneOf;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Anemoi.Hr.Application.Cqrs.Commands.CompensationCommands.CreateAllowanceType;

public sealed class CreateAllowanceTypeHandler(
    ISqlRepository<AllowanceType> allowanceTypeRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<CreateAllowanceTypeCommand, OneOf<CreateAllowanceTypeResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<CreateAllowanceTypeResponse, ErrorDetailResponse>> Handle(
        CreateAllowanceTypeCommand request,
        CancellationToken cancellationToken)
    {
        var cleanCode = request.Code.Trim();
        var exists = await allowanceTypeRepository.ExistByConditionAsync(
            x => x.Code == cleanCode,
            cancellationToken);

        if (exists)
            return HrErrorResponses.Create(HrBusinessErrorCodes.AllowanceTypeCodeAlreadyExists);

        var newType = new AllowanceType
        {
            Id = new AllowanceTypeId(IdGenerator.NextGuid()),
            Code = cleanCode,
            Name = request.Name.Trim(),
            Description = request.Description?.Trim(),
            IsTaxable = request.IsTaxable,
            IsActive = request.IsActive,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await allowanceTypeRepository.CreateOneAsync(newType, cancellationToken);
        var saveResult = await unitOfWork.SaveChangesAsync(cancellationToken);
        
        if (saveResult.IsT1)
            return HrErrorResponses.Create(HrBusinessErrorCodes.SaveChangesFailed);

        return new CreateAllowanceTypeResponse { AllowanceTypeId = newType.Id.Value.ToString() };
    }
}
