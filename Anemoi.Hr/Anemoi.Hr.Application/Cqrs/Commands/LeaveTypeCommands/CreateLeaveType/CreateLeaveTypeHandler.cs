using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.BuildingBlock.Application.Helpers;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Leaves;
using Anemoi.Hr.ModelIds.ModelIds;
using OneOf;

namespace Anemoi.Hr.Application.Cqrs.Commands.LeaveTypeCommands.CreateLeaveType;

public sealed class CreateLeaveTypeHandler(
    ISqlRepository<LeaveType> repository,
    IUnitOfWork unitOfWork,
    MasterDataMapper mapper)
    : ICommandHandler<CreateLeaveTypeCommand, OneOf<LeaveTypeResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<LeaveTypeResponse, ErrorDetailResponse>> Handle(
        CreateLeaveTypeCommand request, CancellationToken cancellationToken)
    {
        var cleanCode = request.Code.Trim();
        var codeExists = await repository.ExistByConditionAsync(
            x => x.Code == cleanCode, cancellationToken);
        if (codeExists)
            return HrErrorResponses.Create(HrBusinessErrorCodes.LeaveTypeCodeAlreadyExists);

        var id = new LeaveTypeId(IdGenerator.NextGuid());
        var entity = LeaveType.Create(id, cleanCode, request.Name.Trim(), request.IsPaid,
            request.RequiresApproval, request.AnnualEntitlement,
            request.CarryForwardAllowed, request.MaxCarryForwardDays);

        await repository.CreateOneAsync(entity, cancellationToken);
        var saveResult = await unitOfWork.SaveChangesAsync(cancellationToken);
        if (saveResult.IsT1)
            return HrErrorResponses.FromSaveResult(saveResult.AsT1, null);

        return mapper.ToLeaveTypeResponse(entity);
    }
}
