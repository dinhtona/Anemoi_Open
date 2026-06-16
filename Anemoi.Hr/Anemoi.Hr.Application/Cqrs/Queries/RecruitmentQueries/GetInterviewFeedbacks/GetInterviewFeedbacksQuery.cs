using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;
using System.Collections.Generic;

namespace Anemoi.Hr.Application.Cqrs.Queries.RecruitmentQueries.GetInterviewFeedbacks;

public sealed record GetInterviewFeedbacksQuery(InterviewScheduleId Id)
    : IQueryOne<IReadOnlyCollection<InterviewFeedbackResponse>>;
