using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Attendance;
using Microsoft.EntityFrameworkCore;
using OneOf;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Anemoi.Hr.Application.Cqrs.Queries.AttendanceQueries.GetAttendancePeriods;

public sealed class GetAttendancePeriodsHandler(
    ISqlRepository<AttendancePeriod> attendancePeriodRepository,
    AttendanceMapper mapper)
    : IQueryHandler<GetAttendancePeriodsQuery, OneOf<IReadOnlyCollection<AttendancePeriodResponse>, ErrorDetailResponse>>
{
    public async Task<OneOf<IReadOnlyCollection<AttendancePeriodResponse>, ErrorDetailResponse>> Handle(
        GetAttendancePeriodsQuery request,
        CancellationToken cancellationToken)
    {
        var periods = await attendancePeriodRepository.GetManyByConditionAsync(
            x => true,
            q => q.OrderByDescending(x => x.PeriodCode),
            cancellationToken);

        return OneOf<IReadOnlyCollection<AttendancePeriodResponse>, ErrorDetailResponse>.FromT0(mapper.ToResponses(periods));
    }
}
