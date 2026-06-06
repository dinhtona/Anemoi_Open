using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Compensation;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Anemoi.Hr.Application.Cqrs.Queries.CompensationQueries.GetAllowanceTypes;

public sealed class GetAllowanceTypesHandler(
    ISqlRepository<AllowanceType> repository,
    CompensationMapper mapper)
    : IQueryHandler<GetAllowanceTypesQuery, IReadOnlyCollection<AllowanceTypeResponse>>
{
    public async Task<IReadOnlyCollection<AllowanceTypeResponse>> Handle(
        GetAllowanceTypesQuery request,
        CancellationToken cancellationToken)
    {
        var types = await repository.GetQueryable()
            .Where(x => x.IsActive)
            .OrderBy(x => x.Code)
            .ToListAsync(cancellationToken);

        return types.Select(mapper.ToAllowanceTypeResponse).ToList();
    }
}
