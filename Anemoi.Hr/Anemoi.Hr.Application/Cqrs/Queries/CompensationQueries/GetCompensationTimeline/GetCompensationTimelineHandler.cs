using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Compensation;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Anemoi.Hr.Application.Cqrs.Queries.CompensationQueries.GetCompensationTimeline;

public sealed class GetCompensationTimelineHandler(
    ISqlRepository<EmployeeSalary> salaryRepository,
    ISqlRepository<EmployeeAllowance> allowanceRepository)
    : IQueryHandler<GetCompensationTimelineQuery, IReadOnlyCollection<CompensationTimelineItemResponse>>
{
    public async Task<IReadOnlyCollection<CompensationTimelineItemResponse>> Handle(
        GetCompensationTimelineQuery request,
        CancellationToken cancellationToken)
    {
        var salaries = await salaryRepository.GetQueryable()
            .Where(x => x.EmployeeId == request.EmployeeId)
            .ToListAsync(cancellationToken);

        var allowances = await allowanceRepository.GetQueryable()
            .Include(x => x.AllowanceType)
            .Where(x => x.EmployeeId == request.EmployeeId)
            .ToListAsync(cancellationToken);

        var items = new List<CompensationTimelineItemResponse>();

        foreach (var salary in salaries)
        {
            items.Add(new CompensationTimelineItemResponse
            {
                Date = salary.EffectiveFrom,
                EventType = "Salary",
                Action = "Changed",
                Description = $"Base salary changed to {salary.BaseSalary:N2} {salary.Currency} ({salary.SalaryType}, Reason: {salary.Reason})",
                Amount = salary.BaseSalary,
                Currency = salary.Currency,
                CreatedBy = salary.CreatedBy,
                CreatedAt = salary.CreatedAt
            });
        }

        foreach (var allowance in allowances)
        {
            items.Add(new CompensationTimelineItemResponse
            {
                Date = allowance.EffectiveFrom,
                EventType = "Allowance",
                Action = "Assigned",
                Description = $"Allowance {allowance.AllowanceType?.Code ?? "Unknown"} assigned: {allowance.Amount:N2} {allowance.Currency}",
                Amount = allowance.Amount,
                Currency = allowance.Currency,
                CreatedBy = allowance.CreatedBy,
                CreatedAt = allowance.CreatedAt
            });

            if (allowance.EffectiveTo.HasValue)
            {
                items.Add(new CompensationTimelineItemResponse
                {
                    Date = allowance.EffectiveTo.Value,
                    EventType = "Allowance",
                    Action = "Terminated",
                    Description = $"Allowance {allowance.AllowanceType?.Code ?? "Unknown"} terminated",
                    Amount = allowance.Amount,
                    Currency = allowance.Currency,
                    CreatedBy = allowance.UpdatedBy,
                    CreatedAt = allowance.UpdatedAt
                });
            }
        }

        return items
            .OrderByDescending(x => x.Date)
            .ThenByDescending(x => x.CreatedAt)
            .ToList();
    }
}
