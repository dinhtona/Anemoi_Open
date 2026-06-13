using System.IO;
using System.Net;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;
using Anemoi.Hr.Application.Abstractions;
using Anemoi.Hr.Application.Configurations;
using Microsoft.Extensions.Logging;

namespace Anemoi.Hr.Infrastructure.Services;

public sealed class SmtpPayslipEmailSender(HrSettings settings, ILogger<SmtpPayslipEmailSender> logger) : IPayslipEmailSender
{
    public async Task SendEmailWithAttachmentAsync(
        string toEmail,
        string subject,
        string body,
        string attachmentFileName,
        byte[] attachmentBytes,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Sending SMTP email to {ToEmail} with attachment {AttachmentFileName} using Host={Host}:{Port}",
            toEmail, attachmentFileName, settings.SmtpHost, settings.SmtpPort);

        using var mailMessage = new MailMessage();
        mailMessage.From = new MailAddress(settings.SmtpSenderEmail, settings.SmtpSenderName);
        mailMessage.To.Add(toEmail);
        mailMessage.Subject = subject;
        mailMessage.Body = body;
        mailMessage.IsBodyHtml = false;

        using var ms = new MemoryStream(attachmentBytes);
        var attachment = new Attachment(ms, attachmentFileName, "application/pdf");
        mailMessage.Attachments.Add(attachment);

        using var smtpClient = new SmtpClient(settings.SmtpHost, settings.SmtpPort);
        smtpClient.EnableSsl = settings.SmtpEnableSsl;
        if (!string.IsNullOrEmpty(settings.SmtpUsername))
        {
            smtpClient.Credentials = new NetworkCredential(settings.SmtpUsername, settings.SmtpPassword);
        }

        await smtpClient.SendMailAsync(mailMessage, cancellationToken);
        
        logger.LogInformation("Successfully sent SMTP email to {ToEmail}", toEmail);
    }
}
