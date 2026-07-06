using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Overtime;

namespace Anemoi.Hr.Application.Cqrs.Queries.OvertimeRequestQueries.GetOvertimeRequestById;

public sealed class GetOvertimeRequestByIdHandler(
    ISqlRepository<OvertimeRequest> overtimeRequestRepository,
    OvertimeMapper mapper)
    : IQueryHandler<GetOvertimeRequestByIdQuery, OvertimeRequestResponse>
{
    public async Task<OvertimeRequestResponse> Handle(GetOvertimeRequestByIdQuery request,
        CancellationToken cancellationToken)
    {
        var overtimeRequest = await overtimeRequestRepository.GetFirstByConditionAsync(
            x => x.Id == request.Id,
            null,
            cancellationToken);

        return overtimeRequest is null ? null : mapper.ToOvertimeRequestResponse(overtimeRequest);
    }
}
