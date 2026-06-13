using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Anemoi.Hr.Application.Abstractions;

public interface IPayslipDocumentStorage
{
    Task SaveAsync(string fileKey, Stream stream, CancellationToken cancellationToken = default);
    Task<Stream> OpenReadAsync(string fileKey, CancellationToken cancellationToken = default);
    Task DeleteAsync(string fileKey, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(string fileKey, CancellationToken cancellationToken = default);
}
