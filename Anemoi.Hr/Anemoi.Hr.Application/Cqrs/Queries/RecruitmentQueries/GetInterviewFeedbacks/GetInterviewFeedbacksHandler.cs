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

namespace Anemoi.Hr.Application.Cqrs.Queries.RecruitmentQueries.GetInterviewFeedbacks;

public sealed class GetInterviewFeedbacksHandler(
    ISqlRepository<InterviewFeedback> feedbackRepository,
    RecruitmentMapper mapper)
    : IQueryHandler<GetInterviewFeedbacksQuery, OneOf<IReadOnlyCollection<InterviewFeedbackResponse>, ErrorDetailResponse>>
{
    public async Task<OneOf<IReadOnlyCollection<InterviewFeedbackResponse>, ErrorDetailResponse>> Handle(
        GetInterviewFeedbacksQuery request,
        CancellationToken cancellationToken)
    {
        var feedbacks = await feedbackRepository.GetManyByConditionAsync(
            x => x.InterviewScheduleId == request.Id,
            q => q.Include(x => x.Interviewer).OrderBy(x => x.CreatedAt),
            cancellationToken);

        return OneOf<IReadOnlyCollection<InterviewFeedbackResponse>, ErrorDetailResponse>
            .FromT0(feedbacks.Select(mapper.ToResponse).ToList());
    }
}
