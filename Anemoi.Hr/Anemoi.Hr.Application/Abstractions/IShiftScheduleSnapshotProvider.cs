using Anemoi.Hr.Application.Responses;

namespace Anemoi.Hr.Application.Abstractions;

public interface IShiftScheduleSnapshotProvider
{
    Task<IEnumerable<EmployeeShiftAssignmentResponse>> GetActiveShiftAssignmentsAsync(
        Guid employeeId,
        DateOnly fromDate,
        DateOnly toDate,
        CancellationToken cancellationToken);
}
