using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Cqrs.Common.Dtos;
using Anemoi.Hr.Domain.Employees;
using Anemoi.Hr.Domain.Probation;
using Anemoi.Hr.Domain.Separations;
using Anemoi.Hr.Domain.Transfers;
using Microsoft.EntityFrameworkCore;
using OneOf;

namespace Anemoi.Hr.Application.Cqrs.Queries.LifecycleQueries.GetLifecycleSummary;

public sealed class GetLifecycleSummaryHandler(
    ISqlRepository<ProbationRecord> probationRepository,
    ISqlRepository<EmployeeTransfer> transferRepository,
    ISqlRepository<EmployeeSeparation> separationRepository,
    ISqlRepository<Employee> employeeRepository)
    : IQueryHandler<GetLifecycleSummaryQuery, OneOf<DashboardLifecycleSummaryDto, ErrorDetailResponse>>
{
    public async Task<OneOf<DashboardLifecycleSummaryDto, ErrorDetailResponse>> Handle(
        GetLifecycleSummaryQuery request, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var today = DateOnly.FromDateTime(now);
        var in7Days = DateOnly.FromDateTime(now.AddDays(7));
        var in14Days = DateOnly.FromDateTime(now.AddDays(14));
        var in30Days = DateOnly.FromDateTime(now.AddDays(30));
        var daysAgo7 = DateOnly.FromDateTime(now.AddDays(-7));
        var daysAgo30 = DateOnly.FromDateTime(now.AddDays(-30));

        var probQuery = probationRepository.GetQueryable()
            .Where(x => x.StatusCode == ProbationStatusCode.Pending && x.EndDate >= today);

        var count7 = await probQuery.CountAsync(x => x.EndDate <= in7Days, cancellationToken);
        var count14 = await probQuery.CountAsync(x => x.EndDate <= in14Days, cancellationToken);
        var count30 = await probQuery.CountAsync(x => x.EndDate <= in30Days, cancellationToken);

        var pendingTransferCount = await transferRepository.GetQueryable()
            .LongCountAsync(x => x.StatusCode == TransferStatusCode.Pending, cancellationToken);

        var pendingSeparationCount = await separationRepository.GetQueryable()
            .LongCountAsync(x => x.StatusCode == SeparationStatusCode.Pending, cancellationToken);

        var created7 = await employeeRepository.GetQueryable()
            .LongCountAsync(x => x.CreatedAt >= daysAgo7.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc), cancellationToken);
        var created30 = await employeeRepository.GetQueryable()
            .LongCountAsync(x => x.CreatedAt >= daysAgo30.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc), cancellationToken);

        return new DashboardLifecycleSummaryDto
        {
            ProbationsExpiring = new ProbationsExpiringDto
            {
                Count7 = count7,
                Count14 = count14,
                Count30 = count30
            },
            PendingTransfers = (int)pendingTransferCount,
            PendingSeparations = (int)pendingSeparationCount,
            NewEmployees = new NewEmployeesDto
            {
                Count7 = (int)created7,
                Count30 = (int)created30
            }
        };
    }
}
