using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Abstractions;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Employees;
using Anemoi.Hr.Domain.Payroll;
using Anemoi.Hr.ModelIds.ModelIds;
using Microsoft.EntityFrameworkCore;
using OneOf;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Anemoi.Hr.Application.Cqrs.Commands.PayslipCommands.SendPayslipEmail;

public sealed class SendPayslipEmailHandler(
    ISqlRepository<Payslip> payslipRepository,
    ISqlRepository<PayslipDocument> payslipDocumentRepository,
    ISqlRepository<PayslipEmailDelivery> payslipEmailDeliveryRepository,
    ISqlRepository<Employee> employeeRepository,
    IUnitOfWork unitOfWork,
    IPayslipEmailSender emailSender,
    IPayslipDocumentStorage documentStorage,
    PayslipDocumentMapper mapper)
    : ICommandHandler<SendPayslipEmailCommand, OneOf<PayslipEmailDeliveryResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<PayslipEmailDeliveryResponse, ErrorDetailResponse>> Handle(
        SendPayslipEmailCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Load Payslip
        var payslip = await payslipRepository.GetFirstByConditionAsync(
            x => x.Id == new PayslipId(request.PayslipId),
            null,
            cancellationToken);

        if (payslip is null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.PayslipNotFound);

        // 2. Validate status (must be Published)
        if (payslip.Status != PayslipStatus.Published)
            return HrErrorResponses.Create(HrBusinessErrorCodes.PayslipEmailSendNotAllowed);

        // 3. Load active document
        var activeDoc = await payslipDocumentRepository.GetFirstByConditionAsync(
            x => x.PayslipId == payslip.Id && x.IsActive,
            null,
            cancellationToken);

        if (activeDoc is null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.PayslipActiveDocumentRequired);

        // 4. Determine recipient email address
        var targetEmail = request.ToEmail;
        if (string.IsNullOrWhiteSpace(targetEmail))
        {
            var employee = await employeeRepository.GetFirstByConditionAsync(
                x => x.Id == payslip.EmployeeId,
                null,
                cancellationToken);

            targetEmail = employee?.WorkEmail;
            if (string.IsNullOrWhiteSpace(targetEmail))
            {
                targetEmail = employee?.PersonalEmail;
            }
        }

        if (string.IsNullOrWhiteSpace(targetEmail))
            return HrErrorResponses.Create(HrBusinessErrorCodes.PayslipEmailRequired);

        // 5. Load file stream and read bytes
        byte[] fileBytes;
        try
        {
            using var fileStream = await documentStorage.OpenReadAsync(activeDoc.StoragePath, cancellationToken);
            using var ms = new MemoryStream();
            await fileStream.CopyToAsync(ms, cancellationToken);
            fileBytes = ms.ToArray();
        }
        catch (Exception)
        {
            return HrErrorResponses.Create(HrBusinessErrorCodes.PayslipDocumentStorageFailed);
        }

        // 6. Audit-First Step A: Create and Save Pending Log
        var delivery = PayslipEmailDelivery.Create(
            payslip.Id,
            activeDoc.Id,
            targetEmail,
            $"Payslip for Period {payslip.PeriodCode}",
            request.SentBy ?? "system");

        var createResult = await payslipEmailDeliveryRepository.CreateOneAsync(delivery, cancellationToken);
        if (createResult.IsT1)
            return HrErrorResponses.Create(HrBusinessErrorCodes.SaveChangesFailed);

        var saveResult = await unitOfWork.SaveChangesAsync(cancellationToken);
        if (saveResult.IsT1)
            return HrErrorResponses.Create(HrBusinessErrorCodes.SaveChangesFailed);

        // 7. Step B: Dispatch email and update log
        try
        {
            await emailSender.SendEmailWithAttachmentAsync(
                targetEmail,
                delivery.Subject,
                $"Dear {payslip.EmployeeName},\n\nPlease find attached your payslip for period {payslip.PeriodCode}.\n\nBest regards,\nHR Department",
                activeDoc.FileName,
                fileBytes,
                cancellationToken);

            delivery.MarkSent(DateTime.UtcNow);
        }
        catch (Exception ex)
        {
            // Store safe summarized error message
            var safeErrorMessage = ex.Message.Length > 2000 ? ex.Message[..2000] : ex.Message;
            delivery.MarkFailed(safeErrorMessage, DateTime.UtcNow);
        }

        // 8. Step C: Save delivery updates
        saveResult = await unitOfWork.SaveChangesAsync(cancellationToken);
        if (saveResult.IsT1)
            return HrErrorResponses.Create(HrBusinessErrorCodes.SaveChangesFailed);

        return mapper.ToResponse(delivery);
    }
}
