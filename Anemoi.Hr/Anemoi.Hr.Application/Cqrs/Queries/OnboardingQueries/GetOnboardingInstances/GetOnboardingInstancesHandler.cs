using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.BuildingBlock.Application.Extensions;
using Anemoi.BuildingBlock.Application.Queries;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Onboarding;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Anemoi.Hr.Application.Cqrs.Queries.OnboardingQueries.GetOnboardingInstances;

public sealed class GetOnboardingInstancesHandler(
    ISqlRepository<OnboardingInstance> instanceRepository,
    OnboardingMapper mapper)
    : IQueryHandler<GetOnboardingInstancesQuery, PaginationResponse<OnboardingInstanceResponse>>
{
    public async Task<PaginationResponse<OnboardingInstanceResponse>> Handle(
        GetOnboardingInstancesQuery request, CancellationToken cancellationToken)
    {
        var page = await instanceRepository.GetManyByConditionWithPaginationAsync(
            x => (request.EmployeeId == null || x.EmployeeId == request.EmployeeId) &&
                 (string.IsNullOrEmpty(request.Status) || x.Status == request.Status) &&
                 (!request.StartDateFrom.HasValue || x.StartDate >= request.StartDateFrom.Value) &&
                 (!request.StartDateTo.HasValue || x.StartDate <= request.StartDateTo.Value),
            q => q.OrderByWithDynamic(request.SortedFieldName, x => x.StartDate,
                    request.SortedDirection ?? SortedDirection.Descending)
                .Offset(request.GetSkip())
                .Limit(request.GetTake()),
            cancellationToken);

        return new PaginationResponse<OnboardingInstanceResponse>(
            page.Items.Select(mapper.ToResponse).ToList(),
            page.TotalRecord);
    }
}
