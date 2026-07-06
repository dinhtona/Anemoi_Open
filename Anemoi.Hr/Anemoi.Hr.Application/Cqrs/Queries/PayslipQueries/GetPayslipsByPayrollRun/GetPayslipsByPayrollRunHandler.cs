using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Payroll;
using OneOf;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Anemoi.Hr.Application.Cqrs.Queries.PayslipQueries.GetPayslipsByPayrollRun;

public sealed class GetPayslipsByPayrollRunHandler(
    ISqlRepository<Payslip> payslipRepository,
    PayslipMapper mapper)
    : IQueryHandler<GetPayslipsByPayrollRunQuery, OneOf<IReadOnlyCollection<PayslipResponse>, ErrorDetailResponse>>
{
    public async Task<OneOf<IReadOnlyCollection<PayslipResponse>, ErrorDetailResponse>> Handle(
        GetPayslipsByPayrollRunQuery request,
        CancellationToken cancellationToken)
    {
        var payslips = await payslipRepository.GetManyByConditionAsync(
            x => x.PayrollRunId == request.PayrollRunId,
            null,
            cancellationToken);

        return OneOf<IReadOnlyCollection<PayslipResponse>, ErrorDetailResponse>.FromT0(mapper.ToResponses(payslips));
    }
}
