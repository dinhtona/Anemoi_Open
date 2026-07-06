using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Attendance;
using Anemoi.Hr.Domain.Employees;
using OneOf;

namespace Anemoi.Hr.Application.Cqrs.Queries.EssQueries.GetMyAttendanceRecords;

public sealed class GetMyAttendanceRecordsHandler(
    ISqlRepository<Employee> employeeRepository,
    ISqlRepository<AttendanceRecord> attendanceRecordRepository,
    EssMapper mapper)
    : IQueryHandler<GetMyAttendanceRecordsQuery, OneOf<IReadOnlyCollection<EssAttendanceRecordResponse>, ErrorDetailResponse>>
{
    public async Task<OneOf<IReadOnlyCollection<EssAttendanceRecordResponse>, ErrorDetailResponse>> Handle(
        GetMyAttendanceRecordsQuery request, CancellationToken cancellationToken)
    {
        Employee employee = null;
        if (Guid.TryParse(request.UserId, out var identityUserId))
            employee = await employeeRepository.GetFirstByConditionAsync(
                x => x.IdentityUserId == identityUserId, null, cancellationToken);

        if (employee is null && !string.IsNullOrEmpty(request.Email))
        {
            var searchEmail = request.Email.ToLower();
            employee = await employeeRepository.GetFirstByConditionAsync(
                x => x.WorkEmail != null && x.WorkEmail.ToLower() == searchEmail, null, cancellationToken);
        }

        if (employee is null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.EmployeeNotFound);

        var records = await attendanceRecordRepository.GetManyByConditionAsync(
            x => x.EmployeeId == employee.Id,
            query => query.OrderByDescending(r => r.WorkDate),
            cancellationToken);

        return records.Select(mapper.ToEssAttendanceRecordResponse).ToList();
    }
}
