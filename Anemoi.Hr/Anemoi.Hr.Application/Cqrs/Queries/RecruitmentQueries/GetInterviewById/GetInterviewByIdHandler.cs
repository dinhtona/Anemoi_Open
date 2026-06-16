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

namespace Anemoi.Hr.Application.Cqrs.Queries.RecruitmentQueries.GetInterviewById;

public sealed class GetInterviewByIdHandler(
    ISqlRepository<InterviewSchedule> interviewRepository,
    RecruitmentMapper mapper)
    : IQueryHandler<GetInterviewByIdQuery, OneOf<InterviewScheduleResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<InterviewScheduleResponse, ErrorDetailResponse>> Handle(
        GetInterviewByIdQuery request,
        CancellationToken cancellationToken)
    {
        var interview = await interviewRepository.GetQueryable()
            .Include(x => x.CandidateApplication).ThenInclude(x => x.Candidate)
            .Include(x => x.CandidateApplication).ThenInclude(x => x.JobPosting)
            .Include(x => x.Interviewer)
            .Include(x => x.Feedbacks)
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (interview is null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.InterviewNotFound);

        return mapper.ToResponse(interview);
    }
}
