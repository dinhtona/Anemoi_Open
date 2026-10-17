using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Employees;
using Microsoft.EntityFrameworkCore;
using OneOf;

namespace Anemoi.Hr.Application.Cqrs.Queries.EmployeeQueries.GetEmployeePositionHistory;

public sealed class GetEmployeePositionHistoryHandler(
    ISqlRepository<EmployeePositionHistory> repository,
    EmployeeMapper mapper)
    : IQueryHandler<GetEmployeePositionHistoryQuery, OneOf<IReadOnlyCollection<EmployeePositionHistoryResponse>, ErrorDetailResponse>>
{
    public async Task<OneOf<IReadOnlyCollection<EmployeePositionHistoryResponse>, ErrorDetailResponse>> Handle(
        GetEmployeePositionHistoryQuery request,
        CancellationToken cancellationToken)
    {
        var histories = await repository.GetQueryable()
            .Include(x => x.Position)
            .Include(x => x.OldPosition)
            .Where(x => x.EmployeeId == request.EmployeeId)
            .OrderByDescending(x => x.EffectiveFrom)
            .ToListAsync(cancellationToken);

        return histories.Select(mapper.ToEmployeePositionHistoryResponse).ToList();
    }
}
