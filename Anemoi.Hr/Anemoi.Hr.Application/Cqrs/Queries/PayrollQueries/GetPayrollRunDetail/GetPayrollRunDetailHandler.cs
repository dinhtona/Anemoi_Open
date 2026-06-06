using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Payroll;
using Microsoft.EntityFrameworkCore;
using OneOf;
using System.Threading;
using System.Threading.Tasks;

namespace Anemoi.Hr.Application.Cqrs.Queries.PayrollQueries.GetPayrollRunDetail;

public sealed class GetPayrollRunDetailHandler(
    ISqlRepository<PayrollRun> payrollRunRepository,
    PayrollMapper mapper)
    : IQueryHandler<GetPayrollRunDetailQuery, OneOf<PayrollRunDetailResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<PayrollRunDetailResponse, ErrorDetailResponse>> Handle(
        GetPayrollRunDetailQuery request,
        CancellationToken cancellationToken)
    {
        var run = await payrollRunRepository.GetQueryable()
            .Include(x => x.PayrollItems)
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (run is null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.PayrollRunNotFound);

        return mapper.ToDetailResponse(run);
    }
}
