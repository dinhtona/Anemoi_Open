using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Recruitment;
using Microsoft.EntityFrameworkCore;
using OneOf;
using System.Threading;
using System.Threading.Tasks;

namespace Anemoi.Hr.Application.Cqrs.Queries.RecruitmentQueries.GetJobPostingById;

public sealed class GetJobPostingByIdHandler(
    ISqlRepository<JobPosting> postingRepository,
    RecruitmentMapper mapper)
    : IQueryHandler<GetJobPostingByIdQuery, OneOf<JobPostingResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<JobPostingResponse, ErrorDetailResponse>> Handle(
        GetJobPostingByIdQuery request,
        CancellationToken cancellationToken)
    {
        var posting = await postingRepository.GetQueryable()
            .Include(x => x.JobRequisition)
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (posting is null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.JobPostingNotFound);

        return mapper.ToResponse(posting);
    }
}
