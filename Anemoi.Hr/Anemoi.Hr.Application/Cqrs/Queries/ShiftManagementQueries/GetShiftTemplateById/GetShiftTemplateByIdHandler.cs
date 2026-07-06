using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.ShiftManagement;

namespace Anemoi.Hr.Application.Cqrs.Queries.ShiftManagementQueries.GetShiftTemplateById;

public sealed class GetShiftTemplateByIdHandler(
    ISqlRepository<ShiftTemplate> shiftTemplateRepository,
    ShiftManagementMapper mapper)
    : IQueryHandler<GetShiftTemplateByIdQuery, ShiftTemplateResponse>
{
    public async Task<ShiftTemplateResponse> Handle(
        GetShiftTemplateByIdQuery request,
        CancellationToken cancellationToken)
    {
        var shiftTemplate = await shiftTemplateRepository.GetFirstByConditionAsync(
            x => x.Id == request.Id, null, cancellationToken);
        return shiftTemplate is null ? null : mapper.ToShiftTemplateResponse(shiftTemplate);
    }
}
