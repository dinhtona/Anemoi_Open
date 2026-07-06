using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.Hr.Application.Abstractions;
using Anemoi.Hr.Domain.Compensation;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Anemoi.Hr.Application.Services;

public sealed class EmployeeGradeLookup(ISqlRepository<SalaryGrade> salaryGradeRepository) : IEmployeeGradeLookup
{
    public async Task<bool> IsValidGradeAsync(string gradeCode, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(gradeCode)) return false;
        var cleanCode = gradeCode.Trim();
        return await salaryGradeRepository.ExistByConditionAsync(
            x => string.Equals(x.GradeCode, cleanCode) && x.IsActive, 
            cancellationToken);
    }
}
