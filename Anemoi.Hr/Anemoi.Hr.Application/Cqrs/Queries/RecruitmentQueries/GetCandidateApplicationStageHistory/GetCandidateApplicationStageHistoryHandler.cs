using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Recruitment;
using Microsoft.EntityFrameworkCore;
using OneOf;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Anemoi.Hr.Application.Cqrs.Queries.RecruitmentQueries.GetCandidateApplicationStageHistory;

public sealed class GetCandidateApplicationStageHistoryHandler(
    ISqlRepository<CandidateApplicationStageHistory> historyRepository,
    RecruitmentMapper mapper)
    : IQueryHandler<GetCandidateApplicationStageHistoryQuery, OneOf<IReadOnlyCollection<CandidateApplicationStageHistoryResponse>, ErrorDetailResponse>>
{
    public async Task<OneOf<IReadOnlyCollection<CandidateApplicationStageHistoryResponse>, ErrorDetailResponse>> Handle(
        GetCandidateApplicationStageHistoryQuery request,
        CancellationToken cancellationToken)
    {
        var exists = await historyRepository.ExistByConditionAsync(
            x => x.CandidateApplicationId == request.Id, cancellationToken);
        if (!exists)
            return HrErrorResponses.Create(HrBusinessErrorCodes.ApplicationNotFound);

        var history = await historyRepository.GetManyByConditionAsync(
            x => x.CandidateApplicationId == request.Id,
            q => q.OrderBy(x => x.ChangedAt),
            cancellationToken);

        return OneOf<IReadOnlyCollection<CandidateApplicationStageHistoryResponse>, ErrorDetailResponse>
            .FromT0(mapper.ToStageHistoryResponses(history));
    }
}
