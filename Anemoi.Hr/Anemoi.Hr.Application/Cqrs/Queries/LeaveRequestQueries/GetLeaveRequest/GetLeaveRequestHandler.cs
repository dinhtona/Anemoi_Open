using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Leaves;
using OneOf;

namespace Anemoi.Hr.Application.Cqrs.Queries.LeaveRequestQueries.GetLeaveRequest;

public sealed class GetLeaveRequestHandler(ISqlRepository<LeaveRequest> repository, LeaveMapper mapper)
    : IQueryHandler<GetLeaveRequestQuery, OneOf<LeaveRequestResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<LeaveRequestResponse, ErrorDetailResponse>> Handle(GetLeaveRequestQuery request,
        CancellationToken cancellationToken)
    {
        var leaveRequest = await repository.GetFirstByConditionAsync(x => x.Id == request.Id, null, cancellationToken);
        return leaveRequest is null
            ? HrErrorResponses.Create(HrBusinessErrorCodes.LeaveRequestNotFound)
            : mapper.ToLeaveRequestResponse(leaveRequest);
    }
}
