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

namespace Anemoi.Hr.Application.Cqrs.Queries.AttendanceQueries.GetAttendanceRecords;

public sealed class GetAttendanceRecordsHandler(
    ISqlRepository<AttendanceRecord> attendanceRecordRepository,
    AttendanceMapper mapper)
    : IQueryHandler<GetAttendanceRecordsQuery, OneOf<IReadOnlyCollection<AttendanceRecordResponse>, ErrorDetailResponse>>
{
    public async Task<OneOf<IReadOnlyCollection<AttendanceRecordResponse>, ErrorDetailResponse>> Handle(
        GetAttendanceRecordsQuery request,
        CancellationToken cancellationToken)
    {
        var query = attendanceRecordRepository.GetQueryable();

        if (request.AttendancePeriodId is not null)
        {
            query = query.Where(x => x.AttendancePeriodId == request.AttendancePeriodId);
        }

        if (request.EmployeeId is not null)
        {
            query = query.Where(x => x.EmployeeId == request.EmployeeId);
        }

        var records = await query
            .Include(x => x.Employee)
            .OrderBy(x => x.WorkDate)
            .ToListAsync(cancellationToken);

        return OneOf<IReadOnlyCollection<AttendanceRecordResponse>, ErrorDetailResponse>.FromT0(mapper.ToResponses(records));
    }
}
