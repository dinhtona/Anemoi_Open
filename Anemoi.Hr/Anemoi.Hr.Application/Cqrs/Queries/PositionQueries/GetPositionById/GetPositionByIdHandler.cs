using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Positions;
using OneOf;

namespace Anemoi.Hr.Application.Cqrs.Queries.PositionQueries.GetPositionById;

public sealed class GetPositionByIdHandler(
    ISqlRepository<Position> repository,
    EmployeeMapper mapper)
    : IQueryHandler<GetPositionByIdQuery, OneOf<PositionResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<PositionResponse, ErrorDetailResponse>> Handle(
        GetPositionByIdQuery request,
        CancellationToken cancellationToken)
    {
        var position = await repository.GetFirstByConditionAsync(
            x => x.Id == request.Id,
            null,
            cancellationToken);

        return position is null
            ? HrErrorResponses.Create(HrBusinessErrorCodes.PositionNotFound)
            : mapper.ToPositionResponse(position);
    }
}
