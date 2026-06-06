using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Payroll;
using Microsoft.EntityFrameworkCore;
using OneOf;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Anemoi.Hr.Application.Cqrs.Queries.PayrollQueries.GetPayrollRuns;

public sealed class GetPayrollRunsHandler(
    ISqlRepository<PayrollPeriod> payrollPeriodRepository,
    ISqlRepository<PayrollRun> payrollRunRepository,
    PayrollMapper mapper)
    : IQueryHandler<GetPayrollRunsQuery, OneOf<IReadOnlyCollection<PayrollRunResponse>, ErrorDetailResponse>>
{
    public async Task<OneOf<IReadOnlyCollection<PayrollRunResponse>, ErrorDetailResponse>> Handle(
        GetPayrollRunsQuery request,
        CancellationToken cancellationToken)
    {
        // 1. Verify payroll period exists
        var periodExists = await payrollPeriodRepository.ExistByConditionAsync(
            x => x.Id == request.PayrollPeriodId,
            cancellationToken);

        if (!periodExists)
            return HrErrorResponses.Create(HrBusinessErrorCodes.PayrollPeriodNotFound);

        // 2. Fetch runs
        var runs = await payrollRunRepository.GetManyByConditionAsync(
            x => x.PayrollPeriodId == request.PayrollPeriodId,
            q => q.OrderBy(x => x.EmployeeCode),
            cancellationToken);

        return OneOf<IReadOnlyCollection<PayrollRunResponse>, ErrorDetailResponse>.FromT0(mapper.ToResponses(runs));
    }
}
