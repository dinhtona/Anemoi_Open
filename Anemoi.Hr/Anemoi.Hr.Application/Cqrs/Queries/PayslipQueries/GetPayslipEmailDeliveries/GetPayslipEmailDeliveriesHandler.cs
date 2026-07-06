using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Payroll;
using Anemoi.Hr.ModelIds.ModelIds;
using OneOf;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Anemoi.Hr.Application.Cqrs.Queries.PayslipQueries.GetPayslipEmailDeliveries;

public sealed class GetPayslipEmailDeliveriesHandler(
    ISqlRepository<Payslip> payslipRepository,
    ISqlRepository<PayslipEmailDelivery> payslipEmailDeliveryRepository,
    PayslipDocumentMapper mapper)
    : IQueryHandler<GetPayslipEmailDeliveriesQuery, OneOf<IReadOnlyCollection<PayslipEmailDeliveryResponse>, ErrorDetailResponse>>
{
    public async Task<OneOf<IReadOnlyCollection<PayslipEmailDeliveryResponse>, ErrorDetailResponse>> Handle(
        GetPayslipEmailDeliveriesQuery request,
        CancellationToken cancellationToken)
    {
        var payslip = await payslipRepository.GetFirstByConditionAsync(
            x => x.Id == new PayslipId(request.PayslipId),
            null,
            cancellationToken);

        if (payslip is null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.PayslipNotFound);

        var deliveries = await payslipEmailDeliveryRepository.GetManyByConditionAsync(
            x => x.PayslipId == payslip.Id,
            query => query.OrderByDescending(d => d.CreatedAt),
            cancellationToken);

        return OneOf<IReadOnlyCollection<PayslipEmailDeliveryResponse>, ErrorDetailResponse>.FromT0(mapper.ToResponses(deliveries));
    }
}
