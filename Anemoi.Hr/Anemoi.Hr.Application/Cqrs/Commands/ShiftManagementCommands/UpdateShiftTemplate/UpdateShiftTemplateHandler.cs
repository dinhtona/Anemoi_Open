using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Microsoft.EntityFrameworkCore;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.ShiftManagement;
using OneOf;

namespace Anemoi.Hr.Application.Cqrs.Commands.ShiftManagementCommands.UpdateShiftTemplate;

public sealed class UpdateShiftTemplateHandler(
    ISqlRepository<ShiftTemplate> shiftTemplateRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateShiftTemplateCommand, OneOf<SuccessResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<SuccessResponse, ErrorDetailResponse>> Handle(
        UpdateShiftTemplateCommand request,
        CancellationToken cancellationToken)
    {
        var shiftTemplate = await shiftTemplateRepository.GetFirstByConditionAsync(
            x => x.Id == request.Id, null, cancellationToken);
        if (shiftTemplate is null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.ShiftTemplateNotFound);

        var duplicateCode = await shiftTemplateRepository.GetQueryable()
            .AnyAsync(x => x.Code == request.Code && x.Id != request.Id, cancellationToken);
        if (duplicateCode)
            return HrErrorResponses.Create(HrBusinessErrorCodes.ShiftTemplateCodeAlreadyExists);

        shiftTemplate.Update(
            request.Code,
            request.Name,
            request.StartTime,
            request.EndTime,
            request.BreakMinutes);

        var saveResult = await unitOfWork.SaveChangesAsync(cancellationToken);
        if (saveResult.IsT1)
        {
            return saveResult.AsT1 is DbUpdateConcurrencyException
                ? HrErrorResponses.Create(HrBusinessErrorCodes.ShiftTemplateConcurrencyConflict)
                : HrErrorResponses.Create("HR_SAVE_CHANGES_FAILED");
        }

        return new SuccessResponse();
    }
}
