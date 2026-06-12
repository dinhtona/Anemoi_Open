using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Microsoft.EntityFrameworkCore;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.ShiftManagement;
using OneOf;

namespace Anemoi.Hr.Application.Cqrs.Commands.ShiftManagementCommands.CreateShiftTemplate;

public sealed class CreateShiftTemplateHandler(
    ISqlRepository<ShiftTemplate> shiftTemplateRepository,
    IUnitOfWork unitOfWork,
    ShiftManagementMapper mapper)
    : ICommandHandler<CreateShiftTemplateCommand, OneOf<ShiftTemplateIdResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<ShiftTemplateIdResponse, ErrorDetailResponse>> Handle(
        CreateShiftTemplateCommand request,
        CancellationToken cancellationToken)
    {
        var existingCode = await shiftTemplateRepository.GetQueryable()
            .AnyAsync(x => x.Code == request.Code, cancellationToken);
        if (existingCode)
            return HrErrorResponses.Create(HrBusinessErrorCodes.ShiftTemplateCodeAlreadyExists);

        var shiftTemplate = ShiftTemplate.Create(
            request.Code,
            request.Name,
            request.StartTime,
            request.EndTime,
            request.BreakMinutes);

        await shiftTemplateRepository.CreateOneAsync(shiftTemplate, cancellationToken);

        var saveResult = await unitOfWork.SaveChangesAsync(cancellationToken);
        if (saveResult.IsT1)
        {
            return saveResult.AsT1 is DbUpdateConcurrencyException
                ? HrErrorResponses.Create(HrBusinessErrorCodes.ShiftTemplateConcurrencyConflict)
                : HrErrorResponses.Create(HrBusinessErrorCodes.SaveChangesFailed);
        }

        return mapper.ToShiftTemplateIdResponse(shiftTemplate);
    }
}
