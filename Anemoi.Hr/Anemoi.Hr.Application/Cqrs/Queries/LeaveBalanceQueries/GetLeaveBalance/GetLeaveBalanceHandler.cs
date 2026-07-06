using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Leaves;
using OneOf;

namespace Anemoi.Hr.Application.Cqrs.Queries.LeaveBalanceQueries.GetLeaveBalance;

public sealed class GetLeaveBalanceHandler(ISqlRepository<LeaveBalance> repository, LeaveMapper mapper)
    : IQueryHandler<GetLeaveBalanceQuery, OneOf<LeaveBalanceResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<LeaveBalanceResponse, ErrorDetailResponse>> Handle(GetLeaveBalanceQuery request,
        CancellationToken cancellationToken)
    {
        var balance = await repository.GetFirstByConditionAsync(x => x.Id == request.Id, null, cancellationToken);
        return balance is null
            ? HrErrorResponses.Create(HrBusinessErrorCodes.LeaveBalanceNotFound)
            : mapper.ToLeaveBalanceResponse(balance);
    }
}
