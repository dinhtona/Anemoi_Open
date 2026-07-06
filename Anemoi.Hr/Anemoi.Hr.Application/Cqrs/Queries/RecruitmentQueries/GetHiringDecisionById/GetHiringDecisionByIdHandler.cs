using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Recruitment;
using Microsoft.EntityFrameworkCore;
using OneOf;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Anemoi.Hr.Application.Cqrs.Queries.RecruitmentQueries.GetHiringDecisionById;

public sealed class GetHiringDecisionByIdHandler(
    ISqlRepository<HiringDecision> decisionRepository,
    RecruitmentMapper mapper)
    : IQueryHandler<GetHiringDecisionByIdQuery, OneOf<HiringDecisionResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<HiringDecisionResponse, ErrorDetailResponse>> Handle(
        GetHiringDecisionByIdQuery request,
        CancellationToken cancellationToken)
    {
        var decision = await decisionRepository.GetQueryable()
            .Include(x => x.CandidateApplication).ThenInclude(x => x.Candidate)
            .Include(x => x.CandidateApplication).ThenInclude(x => x.JobPosting)
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
        if (decision is null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.HiringDecisionNotFound);
        return mapper.ToResponse(decision);
    }
}
