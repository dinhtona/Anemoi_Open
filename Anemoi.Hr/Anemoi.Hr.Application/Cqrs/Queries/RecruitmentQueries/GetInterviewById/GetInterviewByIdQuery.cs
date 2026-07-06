using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Application.Cqrs.Queries.RecruitmentQueries.GetInterviewById;

public sealed record GetInterviewByIdQuery(InterviewScheduleId Id)
    : IQueryOne<InterviewScheduleResponse>;
