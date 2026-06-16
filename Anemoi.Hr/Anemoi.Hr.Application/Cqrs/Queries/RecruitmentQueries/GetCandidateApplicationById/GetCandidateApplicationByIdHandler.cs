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

namespace Anemoi.Hr.Application.Cqrs.Queries.RecruitmentQueries.GetCandidateApplicationById;

public sealed class GetCandidateApplicationByIdHandler(
    ISqlRepository<CandidateApplication> applicationRepository,
    RecruitmentMapper mapper)
    : IQueryHandler<GetCandidateApplicationByIdQuery, OneOf<CandidateApplicationResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<CandidateApplicationResponse, ErrorDetailResponse>> Handle(
        GetCandidateApplicationByIdQuery request,
        CancellationToken cancellationToken)
    {
        var app = await applicationRepository.GetQueryable()
            .Include(x => x.Candidate)
            .Include(x => x.JobPosting).ThenInclude(x => x.JobRequisition)
            .Include(x => x.StageHistories)
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (app is null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.ApplicationNotFound);

        return mapper.ToResponse(app);
    }
}
