using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Overtime;
using OneOf;

namespace Anemoi.Hr.Application.Cqrs.Queries.OvertimeRuleQueries.GetOvertimeRuleById;

public sealed class GetOvertimeRuleByIdHandler(
    ISqlRepository<OvertimeRule> repository,
    MasterDataMapper mapper)
    : IQueryHandler<GetOvertimeRuleByIdQuery, OneOf<OvertimeRuleResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<OvertimeRuleResponse, ErrorDetailResponse>> Handle(
        GetOvertimeRuleByIdQuery request, CancellationToken cancellationToken)
    {
        var entity = await repository.GetFirstByConditionAsync(
            x => x.Id == request.Id, null, cancellationToken);
        return entity is null
            ? HrErrorResponses.Create(HrBusinessErrorCodes.OvertimeRuleNotFound)
            : mapper.ToOvertimeRuleResponse(entity);
    }
}
