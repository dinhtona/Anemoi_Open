using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Employees;
using Microsoft.EntityFrameworkCore;

namespace Anemoi.Hr.Application.Cqrs.Queries.EmployeeQueries.GetEmployeeGradeHistory;

public sealed class GetEmployeeGradeHistoryHandler(
    ISqlRepository<EmployeeGradeHistory> repository,
    EmployeeMapper mapper)
    : IQueryHandler<GetEmployeeGradeHistoryQuery, IReadOnlyCollection<EmployeeGradeHistoryResponse>>
{
    public async Task<IReadOnlyCollection<EmployeeGradeHistoryResponse>> Handle(
        GetEmployeeGradeHistoryQuery request,
        CancellationToken cancellationToken)
    {
        var histories = await repository.GetQueryable()
            .Where(x => x.EmployeeId == request.EmployeeId)
            .OrderByDescending(x => x.EffectiveFrom)
            .ToListAsync(cancellationToken);

        return histories.Select(mapper.ToEmployeeGradeHistoryResponse).ToList();
    }
}
