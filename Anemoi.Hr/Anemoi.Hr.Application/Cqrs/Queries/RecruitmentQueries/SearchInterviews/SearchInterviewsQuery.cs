using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.BuildingBlock.Application.Queries;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;
using System;

namespace Anemoi.Hr.Application.Cqrs.Queries.RecruitmentQueries.SearchInterviews;

public sealed record SearchInterviewsQuery(
    string? SearchTerm,
    string? Result,
    EmployeeId? InterviewerEmployeeId,
    DateTime? FromDate,
    DateTime? ToDate) : GetManyQuery, IQueryOne<PaginationResponse<InterviewScheduleResponse>>;
