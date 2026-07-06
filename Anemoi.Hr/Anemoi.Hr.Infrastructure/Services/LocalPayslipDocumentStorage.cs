using Anemoi.Hr.Application.Abstractions;
using Anemoi.Hr.Application.Configurations;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Anemoi.Hr.Infrastructure.Services;

/// <summary>
/// Local disk storage for Payslip PDFs.
/// 
/// Production Note (Tech Debt):
/// - Production environments should replace this with a durable object storage implementation
///   such as S3, MinIO, Azure Blob, or Google Cloud Storage.
/// </summary>
public sealed class LocalPayslipDocumentStorage(HrSettings settings) : IPayslipDocumentStorage
{
    private string GetFullPath(string fileKey)
    {
        var root = settings.PayslipDocumentStorageRoot;
        if (string.IsNullOrWhiteSpace(root))
        {
            root = "temp_payslip_documents";
        }
        return Path.Combine(Directory.GetCurrentDirectory(), root, fileKey);
    }

    public async Task SaveAsync(string fileKey, Stream stream, CancellationToken cancellationToken = default)
    {
        var fullPath = GetFullPath(fileKey);
        var dir = Path.GetDirectoryName(fullPath);
        if (dir is not null && !Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }

        using var fileStream = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, useAsync: true);
        await stream.CopyToAsync(fileStream, cancellationToken);
    }

    public Task<Stream> OpenReadAsync(string fileKey, CancellationToken cancellationToken = default)
    {
        var fullPath = GetFullPath(fileKey);
        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException("Payslip document file not found.", fullPath);
        }

        var fileStream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync: true);
        return Task.FromResult<Stream>(fileStream);
    }

    public Task DeleteAsync(string fileKey, CancellationToken cancellationToken = default)
    {
        var fullPath = GetFullPath(fileKey);
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }
        return Task.CompletedTask;
    }

    public Task<bool> ExistsAsync(string fileKey, CancellationToken cancellationToken = default)
    {
        var fullPath = GetFullPath(fileKey);
        return Task.FromResult(File.Exists(fullPath));
    }
}
