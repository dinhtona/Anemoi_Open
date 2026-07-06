using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Attendance;
using Microsoft.EntityFrameworkCore;
using OneOf;
using System.Threading;
using System.Threading.Tasks;

namespace Anemoi.Hr.Application.Cqrs.Queries.AttendanceQueries.GetAttendanceRecordDetail;

public sealed class GetAttendanceRecordDetailHandler(
    ISqlRepository<AttendanceRecord> attendanceRecordRepository,
    AttendanceMapper mapper)
    : IQueryHandler<GetAttendanceRecordDetailQuery, OneOf<AttendanceRecordDetailResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<AttendanceRecordDetailResponse, ErrorDetailResponse>> Handle(
        GetAttendanceRecordDetailQuery request,
        CancellationToken cancellationToken)
    {
        var record = await attendanceRecordRepository.GetFirstByConditionAsync(
            x => x.Id == request.Id,
            q => q.Include(x => x.Employee).Include(x => x.AttendancePeriod),
            cancellationToken);

        if (record is null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.AttendanceRecordNotFound);

        return mapper.ToDetailResponse(record);
    }
}
