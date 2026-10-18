using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.CalendarManagement;
using OneOf;

namespace Anemoi.Hr.Application.Cqrs.Queries.CalendarManagementQueries.WorkingCalendarRule.GetWorkingCalendarRuleById;

public sealed class GetWorkingCalendarRuleByIdHandler(
    ISqlRepository<Domain.CalendarManagement.WorkingCalendarRule> workingCalendarRuleRepository,
    CalendarManagementMapper mapper)
    : IQueryHandler<GetWorkingCalendarRuleByIdQuery, OneOf<WorkingCalendarRuleResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<WorkingCalendarRuleResponse, ErrorDetailResponse>> Handle(
        GetWorkingCalendarRuleByIdQuery request,
        CancellationToken cancellationToken)
    {
        var workingCalendarRule = await workingCalendarRuleRepository.GetFirstByConditionAsync(
            x => x.Id == request.Id, null, cancellationToken);
        return workingCalendarRule is null ? null : mapper.ToWorkingCalendarRuleResponse(workingCalendarRule);
    }
}
