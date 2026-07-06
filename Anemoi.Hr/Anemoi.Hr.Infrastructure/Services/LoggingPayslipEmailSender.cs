using Anemoi.Hr.Application.Abstractions;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace Anemoi.Hr.Infrastructure.Services;

public sealed class LoggingPayslipEmailSender(ILogger<LoggingPayslipEmailSender> logger) : IPayslipEmailSender
{
    public Task SendEmailWithAttachmentAsync(
        string toEmail,
        string subject,
        string body,
        string attachmentFileName,
        byte[] attachmentBytes,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "Sending payslip email to {ToEmail} with subject '{Subject}'. Body preview: {BodyPreview}. Attachment: {FileName} ({Size} bytes)",
            toEmail,
            subject,
            body.Length > 100 ? body[..100] + "..." : body,
            attachmentFileName,
            attachmentBytes?.Length ?? 0);

        return Task.CompletedTask;
    }
}
