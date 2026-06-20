using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Leaves;
using OneOf;

namespace Anemoi.Hr.Application.Cqrs.Queries.LeaveTypeQueries.GetLeaveTypeById;

public sealed class GetLeaveTypeByIdHandler(
    ISqlRepository<LeaveType> repository,
    MasterDataMapper mapper)
    : IQueryHandler<GetLeaveTypeByIdQuery, OneOf<LeaveTypeResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<LeaveTypeResponse, ErrorDetailResponse>> Handle(
        GetLeaveTypeByIdQuery request, CancellationToken cancellationToken)
    {
        var entity = await repository.GetFirstByConditionAsync(
            x => x.Id == request.Id, null, cancellationToken);
        return entity is null
            ? HrErrorResponses.Create(HrBusinessErrorCodes.LeaveTypeNotFound)
            : mapper.ToLeaveTypeResponse(entity);
    }
}
