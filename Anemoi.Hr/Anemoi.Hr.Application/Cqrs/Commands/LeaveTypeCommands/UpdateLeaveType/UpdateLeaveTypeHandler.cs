using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Leaves;
using OneOf;

namespace Anemoi.Hr.Application.Cqrs.Commands.LeaveTypeCommands.UpdateLeaveType;

public sealed class UpdateLeaveTypeHandler(
    ISqlRepository<LeaveType> repository,
    IUnitOfWork unitOfWork,
    MasterDataMapper mapper)
    : ICommandHandler<UpdateLeaveTypeCommand, OneOf<LeaveTypeResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<LeaveTypeResponse, ErrorDetailResponse>> Handle(
        UpdateLeaveTypeCommand request, CancellationToken cancellationToken)
    {
        var entity = await repository.GetFirstByConditionAsync(
            x => x.Id == request.Id, null, cancellationToken);
        if (entity is null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.LeaveTypeNotFound);

        var cleanCode = request.Code.Trim();
        var codeExists = await repository.ExistByConditionAsync(
            x => x.Code == cleanCode && x.Id != request.Id, cancellationToken);
        if (codeExists)
            return HrErrorResponses.Create(HrBusinessErrorCodes.LeaveTypeCodeAlreadyExists);

        entity.UpdateInfo(cleanCode, request.Name.Trim(), request.IsPaid,
            request.RequiresApproval, request.AnnualEntitlement,
            request.CarryForwardAllowed, request.MaxCarryForwardDays);

        var saveResult = await unitOfWork.SaveChangesAsync(cancellationToken);
        if (saveResult.IsT1)
            return HrErrorResponses.FromSaveResult(saveResult.AsT1, null);

        return mapper.ToLeaveTypeResponse(entity);
    }
}
