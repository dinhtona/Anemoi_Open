using System.IO;

namespace Anemoi.Hr.Application.Responses;

public sealed class PayslipDocumentDownloadResponse
{
    public Stream Stream { get; set; } = default!;
    public string FileName { get; set; } = default!;
    public string ContentType { get; set; } = default!;
}
