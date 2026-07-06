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

        var totalsByCurrency = activeAllowances
            .GroupBy(x => x.Currency, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                x => x.Key,
                x => x.Sum(a => a.Amount),
                StringComparer.OrdinalIgnoreCase);

        if (activeSalary is not null)
        {
            var salaryCost = activeSalary.SalaryType == SalaryType.Monthly
                ? activeSalary.BaseSalary
                : activeSalary.BaseSalary * 21.75m;

            if (totalsByCurrency.TryGetValue(activeSalary.Currency, out var existingTotal))
                totalsByCurrency[activeSalary.Currency] = existingTotal + salaryCost;
            else
                totalsByCurrency[activeSalary.Currency] = salaryCost;
        }

        return new CompensationSnapshotResponse
        {
            ActiveSalary = mapper.ToEmployeeSalaryResponse(activeSalary),
            ActiveAllowances = activeAllowances.Select(mapper.ToEmployeeAllowanceResponse).ToList(),
            BaseSalary = activeSalary is null
                ? null
                : new SalaryAmountResponse
                {
                    Amount = activeSalary.SalaryType == SalaryType.Monthly
                        ? activeSalary.BaseSalary
                        : activeSalary.BaseSalary * 21.75m,
                    Currency = activeSalary.Currency
                },
            Allowances = activeAllowances
                .GroupBy(x => x.Currency, StringComparer.OrdinalIgnoreCase)
                .Select(x => new CurrencyTotalResponse
                {
                    Currency = x.Key,
                    TotalAmount = x.Sum(a => a.Amount)
                })
                .OrderBy(x => x.Currency)
                .ToList(),
            TotalsByCurrency = totalsByCurrency
                .Select(x => new CurrencyTotalResponse
                {
                    Currency = x.Key,
                    TotalAmount = x.Value
                })
                .OrderBy(x => x.Currency)
                .ToList()
        };
    }
}
