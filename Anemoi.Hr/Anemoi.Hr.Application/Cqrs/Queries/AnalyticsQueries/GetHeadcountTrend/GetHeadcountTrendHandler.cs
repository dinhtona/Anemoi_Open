using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Employees;
using Microsoft.EntityFrameworkCore;

namespace Anemoi.Hr.Application.Cqrs.Queries.AnalyticsQueries.GetHeadcountTrend;

public sealed class GetHeadcountTrendHandler(
    ISqlRepository<Employee> employeeRepository)
    : IQueryHandler<GetHeadcountTrendQuery, ICollection<HeadcountTrendItem>>
{
    public async Task<ICollection<HeadcountTrendItem>> Handle(
        GetHeadcountTrendQuery request,
        CancellationToken cancellationToken)
    {
        var endDate = new DateTime(request.ToDate.Year, request.ToDate.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(1);
        var monthlyJoins = await employeeRepository.GetQueryable()
            .Where(x => x.CreatedAt < endDate)
            .Select(x => new { x.CreatedAt, x.EmploymentStatusCode })
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var trend = new List<HeadcountTrendItem>();
        var current = new DateOnly(request.FromDate.Year, request.FromDate.Month, 1);
        var end = new DateOnly(request.ToDate.Year, request.ToDate.Month, 1);

        while (current <= end)
        {
            var monthEnd = current.AddMonths(1);
            var headcount = monthlyJoins.Count(e =>
                DateOnly.FromDateTime(e.CreatedAt) < monthEnd &&
                e.EmploymentStatusCode == "active");
            trend.Add(new HeadcountTrendItem(current, headcount));
            current = current.AddMonths(1);
        }

        return trend;
    }
}
