using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.BuildingBlock.Application.Helpers;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Payroll;
using OneOf;
using System.Threading;
using System.Threading.Tasks;

namespace Anemoi.Hr.Application.Cqrs.Queries.PayslipQueries.GetPayslipDetail;

public sealed class GetPayslipDetailHandler(
    ISqlRepository<Payslip> payslipRepository,
    PayslipMapper mapper)
    : IQueryHandler<GetPayslipDetailQuery, OneOf<PayslipResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<PayslipResponse, ErrorDetailResponse>> Handle(
        GetPayslipDetailQuery request,
        CancellationToken cancellationToken)
    {
        var payslip = await payslipRepository.GetFirstByConditionAsync(
            x => x.Id == request.PayslipId,
            null,
            cancellationToken);

        if (payslip is null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.PayslipNotFound);

        return mapper.ToResponse(payslip);
    }
}
