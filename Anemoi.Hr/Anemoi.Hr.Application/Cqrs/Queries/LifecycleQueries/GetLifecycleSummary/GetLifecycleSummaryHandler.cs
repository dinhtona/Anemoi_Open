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
        var thirtyDaysFromNow = DateOnly.FromDateTime(now.AddDays(30));

        var expiringProbationCount = await probationRepository.GetQueryable()
            .LongCountAsync(x => x.StatusCode == ProbationStatusCode.Pending && x.EndDate <= thirtyDaysFromNow, cancellationToken);

        var pendingTransferCount = await transferRepository.GetQueryable()
            .LongCountAsync(x => x.StatusCode == TransferStatusCode.Pending, cancellationToken);

        var pendingSeparationCount = await separationRepository.GetQueryable()
            .LongCountAsync(x => x.StatusCode == SeparationStatusCode.Pending, cancellationToken);

        var lastMonth = now.AddMonths(-1);
        var newEmployeeCount = await employeeRepository.GetQueryable()
            .LongCountAsync(x => x.JoinDate >= DateOnly.FromDateTime(lastMonth), cancellationToken);

        var activeProbationCount = await probationRepository.GetQueryable()
            .LongCountAsync(x => x.StatusCode == ProbationStatusCode.Pending, cancellationToken);

        var activeTransferCount = await transferRepository.GetQueryable()
            .LongCountAsync(x => x.StatusCode == TransferStatusCode.Draft || x.StatusCode == TransferStatusCode.Pending, cancellationToken);

        var activeSeparationCount = await separationRepository.GetQueryable()
            .LongCountAsync(x => x.StatusCode == SeparationStatusCode.Draft || x.StatusCode == SeparationStatusCode.Pending, cancellationToken);

        return new DashboardLifecycleSummaryDto
        {
            ExpiringProbationCount = (int)expiringProbationCount,
            PendingTransferCount = (int)pendingTransferCount,
            PendingSeparationCount = (int)pendingSeparationCount,
            NewEmployeeCount = (int)newEmployeeCount,
            ActiveProbationCount = (int)activeProbationCount,
            ActiveTransferCount = (int)activeTransferCount,
            ActiveSeparationCount = (int)activeSeparationCount
        };
    }
}
