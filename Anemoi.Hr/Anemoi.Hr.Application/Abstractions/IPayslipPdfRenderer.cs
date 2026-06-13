using Anemoi.Hr.Domain.Payroll;
using System.Threading;
using System.Threading.Tasks;

namespace Anemoi.Hr.Application.Abstractions;

public interface IPayslipPdfRenderer
{
    Task<byte[]> RenderAsync(Payslip payslip, CancellationToken cancellationToken = default);
}
