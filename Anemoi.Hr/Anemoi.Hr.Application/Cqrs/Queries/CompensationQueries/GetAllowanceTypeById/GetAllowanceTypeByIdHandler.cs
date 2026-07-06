using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Compensation;
using OneOf;

namespace Anemoi.Hr.Application.Cqrs.Queries.CompensationQueries.GetAllowanceTypeById;

public sealed class GetAllowanceTypeByIdHandler(
    ISqlRepository<AllowanceType> repository,
    CompensationMapper mapper)
    : IQueryHandler<GetAllowanceTypeByIdQuery, OneOf<AllowanceTypeResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<AllowanceTypeResponse, ErrorDetailResponse>> Handle(
        GetAllowanceTypeByIdQuery request, CancellationToken cancellationToken)
    {
        var entity = await repository.GetFirstByConditionAsync(
            x => x.Id == request.Id, null, cancellationToken);
        return entity is null
            ? HrErrorResponses.Create(HrBusinessErrorCodes.AllowanceTypeNotFound)
            : mapper.ToAllowanceTypeResponse(entity);
    }
}
