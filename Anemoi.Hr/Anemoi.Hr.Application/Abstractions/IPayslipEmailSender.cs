using System.Threading;
using System.Threading.Tasks;

namespace Anemoi.Hr.Application.Abstractions;

public interface IPayslipEmailSender
{
    Task SendEmailWithAttachmentAsync(
        string toEmail,
        string subject,
        string body,
        string attachmentFileName,
        byte[] attachmentBytes,
        CancellationToken cancellationToken = default);
}
