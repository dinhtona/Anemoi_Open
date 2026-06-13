using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Abstractions;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Payroll;
using Anemoi.Hr.ModelIds.ModelIds;
using OneOf;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Anemoi.Hr.Application.Cqrs.Queries.PayslipQueries.GetPayslipDocumentDownload;

public sealed class GetPayslipDocumentDownloadHandler(
    ISqlRepository<Payslip> payslipRepository,
    ISqlRepository<PayslipDocument> payslipDocumentRepository,
    IPayslipDocumentStorage documentStorage)
    : IQueryHandler<GetPayslipDocumentDownloadQuery, OneOf<PayslipDocumentDownloadResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<PayslipDocumentDownloadResponse, ErrorDetailResponse>> Handle(
        GetPayslipDocumentDownloadQuery request,
        CancellationToken cancellationToken)
    {
        // 1. Validate payslip exists
        var payslip = await payslipRepository.GetFirstByConditionAsync(
            x => x.Id == new PayslipId(request.PayslipId),
            null,
            cancellationToken);

        if (payslip is null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.PayslipNotFound);

        // 2. Validate document exists
        var doc = await payslipDocumentRepository.GetFirstByConditionAsync(
            x => x.Id == new PayslipDocumentId(request.DocumentId),
            null,
            cancellationToken);

        if (doc is null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.PayslipDocumentNotFound);

        // 3. Validate document belongs to the payslip
        if (doc.PayslipId != payslip.Id)
            return HrErrorResponses.Create(HrBusinessErrorCodes.PayslipDocumentNotFound);

        // 4. Validate physical file exists in storage
        var exists = await documentStorage.ExistsAsync(doc.StoragePath, cancellationToken);
        if (!exists)
            return HrErrorResponses.Create(HrBusinessErrorCodes.PayslipDocumentNotFound);

        // 5. Open stream for download
        Stream fileStream;
        try
        {
            fileStream = await documentStorage.OpenReadAsync(doc.StoragePath, cancellationToken);
        }
        catch (Exception)
        {
            return HrErrorResponses.Create(HrBusinessErrorCodes.PayslipDocumentStorageFailed);
        }

        return new PayslipDocumentDownloadResponse
        {
            Stream = fileStream,
            FileName = doc.FileName,
            ContentType = doc.ContentType
        };
    }
}
