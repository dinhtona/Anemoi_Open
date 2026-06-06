using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Compensation;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Anemoi.Hr.Application.Cqrs.Queries.CompensationQueries.GetCompensationSnapshot;

public sealed class GetCompensationSnapshotHandler(
    ISqlRepository<EmployeeSalary> salaryRepository,
    ISqlRepository<EmployeeAllowance> allowanceRepository,
    CompensationMapper mapper)
    : IQueryHandler<GetCompensationSnapshotQuery, CompensationSnapshotResponse>
{
    public async Task<CompensationSnapshotResponse> Handle(
        GetCompensationSnapshotQuery request,
        CancellationToken cancellationToken)
    {
        var refDate = request.ReferenceDate;

        var activeSalary = await salaryRepository.GetQueryable()
            .Where(x => x.EmployeeId == request.EmployeeId &&
                        x.EffectiveFrom <= refDate &&
                        (x.EffectiveTo == null || x.EffectiveTo >= refDate))
            .FirstOrDefaultAsync(cancellationToken);

        var activeAllowances = await allowanceRepository.GetQueryable()
            .Include(x => x.AllowanceType)
            .Where(x => x.EmployeeId == request.EmployeeId &&
                        x.EffectiveFrom <= refDate &&
                        (x.EffectiveTo == null || x.EffectiveTo >= refDate))
            .ToListAsync(cancellationToken);

        decimal totalMonthlyCost = 0;

        if (activeSalary is not null)
        {
            var salaryCost = activeSalary.SalaryType == SalaryType.Monthly
                ? activeSalary.BaseSalary
                : activeSalary.BaseSalary * 21.75m;

            totalMonthlyCost += salaryCost;

            // Sum allowances that match the salary currency
            var matchingAllowances = activeAllowances
                .Where(x => string.Equals(x.Currency, activeSalary.Currency, StringComparison.OrdinalIgnoreCase));

            totalMonthlyCost += matchingAllowances.Sum(x => x.Amount);
        }
        else if (activeAllowances.Any())
        {
            // Sum all allowances since there's no salary (assuming they are in the same currency or just raw sum)
            totalMonthlyCost += activeAllowances.Sum(x => x.Amount);
        }

        return new CompensationSnapshotResponse
        {
            ActiveSalary = mapper.ToEmployeeSalaryResponse(activeSalary),
            ActiveAllowances = activeAllowances.Select(mapper.ToEmployeeAllowanceResponse).ToList(),
            TotalMonthlyCost = totalMonthlyCost
        };
    }
}
