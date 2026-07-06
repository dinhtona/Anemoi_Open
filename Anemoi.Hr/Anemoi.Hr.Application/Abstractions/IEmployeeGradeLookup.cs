using System.Threading;
using System.Threading.Tasks;

namespace Anemoi.Hr.Application.Abstractions;

public interface IEmployeeGradeLookup
{
    Task<bool> IsValidGradeAsync(string gradeCode, CancellationToken cancellationToken);
}
