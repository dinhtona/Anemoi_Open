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

namespace Anemoi.Hr.Application.Cqrs.Queries.PayslipQueries.GetPayslipDocuments;

public sealed class GetPayslipDocumentsHandler(
    ISqlRepository<Payslip> payslipRepository,
    ISqlRepository<PayslipDocument> payslipDocumentRepository,
    PayslipDocumentMapper mapper)
    : IQueryHandler<GetPayslipDocumentsQuery, OneOf<IReadOnlyCollection<PayslipDocumentResponse>, ErrorDetailResponse>>
{
    public async Task<OneOf<IReadOnlyCollection<PayslipDocumentResponse>, ErrorDetailResponse>> Handle(
        GetPayslipDocumentsQuery request,
        CancellationToken cancellationToken)
    {
        var payslip = await payslipRepository.GetFirstByConditionAsync(
            x => x.Id == new PayslipId(request.PayslipId),
            null,
            cancellationToken);

        if (payslip is null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.PayslipNotFound);

        var docs = await payslipDocumentRepository.GetManyByConditionAsync(
            x => x.PayslipId == payslip.Id,
            query => query.OrderByDescending(d => d.Version),
            cancellationToken);

        return OneOf<IReadOnlyCollection<PayslipDocumentResponse>, ErrorDetailResponse>.FromT0(mapper.ToResponses(docs));
    }
}
