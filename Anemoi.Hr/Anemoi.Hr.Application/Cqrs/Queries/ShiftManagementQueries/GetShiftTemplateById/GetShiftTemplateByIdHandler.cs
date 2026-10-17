using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.ShiftManagement;
using OneOf;

namespace Anemoi.Hr.Application.Cqrs.Queries.ShiftManagementQueries.GetShiftTemplateById;

public sealed class GetShiftTemplateByIdHandler(
    ISqlRepository<ShiftTemplate> shiftTemplateRepository,
    ShiftManagementMapper mapper)
    : IQueryHandler<GetShiftTemplateByIdQuery, OneOf<ShiftTemplateResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<ShiftTemplateResponse, ErrorDetailResponse>> Handle(
        GetShiftTemplateByIdQuery request,
        CancellationToken cancellationToken)
    {
        var shiftTemplate = await shiftTemplateRepository.GetFirstByConditionAsync(
            x => x.Id == request.Id, null, cancellationToken);
        return shiftTemplate is null ? null : mapper.ToShiftTemplateResponse(shiftTemplate);
    }
}
