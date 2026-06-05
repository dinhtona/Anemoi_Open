using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Employees;
using Microsoft.EntityFrameworkCore;

namespace Anemoi.Hr.Application.Cqrs.Queries.EmployeeQueries.GetEmployeePromotionTimeline;

public sealed class GetEmployeePromotionTimelineHandler(
    ISqlRepository<EmployeePositionHistory> positionHistoryRepository,
    ISqlRepository<EmployeeGradeHistory> gradeHistoryRepository)
    : IQueryHandler<GetEmployeePromotionTimelineQuery, IReadOnlyCollection<EmployeePromotionTimelineResponse>>
{
    public async Task<IReadOnlyCollection<EmployeePromotionTimelineResponse>> Handle(
        GetEmployeePromotionTimelineQuery request,
        CancellationToken cancellationToken)
    {
        var positionHistories = await positionHistoryRepository.GetQueryable()
            .Include(x => x.Position)
            .Include(x => x.OldPosition)
            .Where(x => x.EmployeeId == request.EmployeeId)
            .ToListAsync(cancellationToken);

        var gradeHistories = await gradeHistoryRepository.GetQueryable()
            .Where(x => x.EmployeeId == request.EmployeeId)
            .ToListAsync(cancellationToken);

        return positionHistories
            .Select(ToPositionTimeline)
            .Concat(gradeHistories.Select(ToGradeTimeline))
            .OrderByDescending(x => x.EffectiveFrom)
            .ThenByDescending(x => x.CreatedAt)
            .ToList();
    }

    private static EmployeePromotionTimelineResponse ToPositionTimeline(EmployeePositionHistory history)
    {
        return new EmployeePromotionTimelineResponse
        {
            Id = history.Id.Value.ToString(),
            EmployeeId = history.EmployeeId.Value.ToString(),
            ChangeType = "position",
            OldValueId = history.OldPositionId?.Value.ToString(),
            OldValueCode = history.OldPosition?.Code,
            OldValueName = history.OldPosition?.Name,
            NewValueId = history.PositionId.Value.ToString(),
            NewValueCode = history.Position?.Code,
            NewValueName = history.Position?.Name,
            EffectiveFrom = history.EffectiveFrom,
            EffectiveTo = history.EffectiveTo,
            ReasonCode = history.ReasonCode,
            CreatedBy = history.CreatedBy,
            CreatedAt = history.CreatedAt
        };
    }

    private static EmployeePromotionTimelineResponse ToGradeTimeline(EmployeeGradeHistory history)
    {
        return new EmployeePromotionTimelineResponse
        {
            Id = history.Id.Value.ToString(),
            EmployeeId = history.EmployeeId.Value.ToString(),
            ChangeType = "grade",
            OldValueCode = history.OldGradeCode,
            OldValueName = history.OldGradeCode,
            NewValueCode = history.GradeCode,
            NewValueName = history.GradeCode,
            EffectiveFrom = history.EffectiveFrom,
            EffectiveTo = history.EffectiveTo,
            ReasonCode = history.ReasonCode,
            CreatedBy = history.CreatedBy,
            CreatedAt = history.CreatedAt
        };
    }
}
