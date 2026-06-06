using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Payroll;
using Microsoft.EntityFrameworkCore;
using OneOf;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Anemoi.Hr.Application.Cqrs.Queries.PayrollQueries.GetPayrollPeriods;

public sealed class GetPayrollPeriodsHandler(
    ISqlRepository<PayrollPeriod> payrollPeriodRepository,
    PayrollMapper mapper)
    : IQueryHandler<GetPayrollPeriodsQuery, OneOf<IReadOnlyCollection<PayrollPeriodResponse>, ErrorDetailResponse>>
{
    public async Task<OneOf<IReadOnlyCollection<PayrollPeriodResponse>, ErrorDetailResponse>> Handle(
        GetPayrollPeriodsQuery request,
        CancellationToken cancellationToken)
    {
        var periods = await payrollPeriodRepository.GetManyByConditionAsync(
            x => true,
            q => q.OrderByDescending(x => x.PeriodCode),
            cancellationToken);

        return OneOf<IReadOnlyCollection<PayrollPeriodResponse>, ErrorDetailResponse>.FromT0(mapper.ToResponses(periods));
    }
}
