using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Abstractions;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Payroll;
using Anemoi.Hr.ModelIds.ModelIds;
using Microsoft.EntityFrameworkCore;
using OneOf;
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace Anemoi.Hr.Application.Cqrs.Commands.PayslipCommands.GeneratePayslipPdf;

public sealed class GeneratePayslipPdfHandler(
    ISqlRepository<Payslip> payslipRepository,
    ISqlRepository<PayslipDocument> payslipDocumentRepository,
    IUnitOfWork unitOfWork,
    IPayslipPdfRenderer pdfRenderer,
    IPayslipDocumentStorage documentStorage,
    PayslipDocumentMapper mapper)
    : ICommandHandler<GeneratePayslipPdfCommand, OneOf<PayslipDocumentResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<PayslipDocumentResponse, ErrorDetailResponse>> Handle(
        GeneratePayslipPdfCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Get payslip
        var payslip = await payslipRepository.GetFirstByConditionAsync(
            x => x.Id == new PayslipId(request.PayslipId),
            null,
            cancellationToken);

        if (payslip is null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.PayslipNotFound);

        // 2. Validate payslip status allows PDF generation
        if (payslip.Status != PayslipStatus.Generated && payslip.Status != PayslipStatus.Published)
            return HrErrorResponses.Create(HrBusinessErrorCodes.PayslipDocumentGenerationNotAllowed);

        // 3. Check for existing active document
        var activeDoc = await payslipDocumentRepository.GetFirstByConditionAsync(
            x => x.PayslipId == payslip.Id && x.IsActive,
            null,
            cancellationToken);

        if (activeDoc is not null && !request.ForceRegenerate)
        {
            return mapper.ToResponse(activeDoc);
        }

        // 4. Calculate next version
        int nextVersion = 1;
        if (activeDoc is not null)
        {
            var maxVersionDoc = await payslipDocumentRepository.GetFirstByConditionAsync(
                x => x.PayslipId == payslip.Id,
                query => query.OrderByDescending(d => d.Version),
                cancellationToken);

            nextVersion = (maxVersionDoc?.Version ?? 0) + 1;
        }

        // 5. Render PDF
        byte[] pdfBytes;
        try
        {
            pdfBytes = await pdfRenderer.RenderAsync(payslip, cancellationToken);
        }
        catch (Exception)
        {
            return HrErrorResponses.Create(HrBusinessErrorCodes.PayslipDocumentGenerationFailed);
        }

        // 6. Calculate checksum
        string checksumHash;
        using (var sha256 = SHA256.Create())
        {
            var hashBytes = sha256.ComputeHash(pdfBytes);
            checksumHash = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
        }

        // 7. Save PDF to storage
        var fileKey = $"{payslip.Id.Value}/v{nextVersion}.pdf";
        try
        {
            using var stream = new MemoryStream(pdfBytes);
            await documentStorage.SaveAsync(fileKey, stream, cancellationToken);
        }
        catch (Exception)
        {
            return HrErrorResponses.Create(HrBusinessErrorCodes.PayslipDocumentStorageFailed);
        }

        // 8. Deactivate current active document if force-regenerating
        if (activeDoc is not null && request.ForceRegenerate)
        {
            activeDoc.Deactivate();
        }

        // 9. Save new document to database
        var fileName = $"payslip_{payslip.PeriodCode}_{payslip.EmployeeCode}_v{nextVersion}.pdf";
        var doc = PayslipDocument.Create(
            payslip.Id,
            fileName,
            "application/pdf",
            fileKey,
            pdfBytes.Length,
            checksumHash,
            request.GeneratedBy ?? "system",
            DateTime.UtcNow,
            nextVersion);

        var createResult = await payslipDocumentRepository.CreateOneAsync(doc, cancellationToken);
        if (createResult.IsT1)
            return HrErrorResponses.Create(HrBusinessErrorCodes.SaveChangesFailed);

        var saveResult = await unitOfWork.SaveChangesAsync(cancellationToken);
        if (saveResult.IsT1)
            return HrErrorResponses.Create(HrBusinessErrorCodes.SaveChangesFailed);

        return mapper.ToResponse(doc);
    }
}
