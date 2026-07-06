using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Employees;
using Anemoi.Hr.Domain.Payroll;
using Microsoft.EntityFrameworkCore;
using OneOf;

namespace Anemoi.Hr.Application.Cqrs.Queries.EssQueries.GetMyPayrollHistory;

public sealed class GetMyPayrollHistoryHandler(
    ISqlRepository<Employee> employeeRepository,
    ISqlRepository<Payslip> payslipRepository,
    ISqlRepository<PayrollPeriod> payrollPeriodRepository,
    EssMapper mapper)
    : IQueryHandler<GetMyPayrollHistoryQuery, OneOf<IReadOnlyCollection<EssPayrollPeriodResponse>, ErrorDetailResponse>>
{
    public async Task<OneOf<IReadOnlyCollection<EssPayrollPeriodResponse>, ErrorDetailResponse>> Handle(
        GetMyPayrollHistoryQuery request, CancellationToken cancellationToken)
    {
        Employee employee = null;
        if (Guid.TryParse(request.UserId, out var identityUserId))
            employee = await employeeRepository.GetFirstByConditionAsync(
                x => x.IdentityUserId == identityUserId, null, cancellationToken);

        if (employee is null && !string.IsNullOrEmpty(request.Email))
        {
            var searchEmail = request.Email.ToLower();
            employee = await employeeRepository.GetFirstByConditionAsync(
                x => x.WorkEmail != null && x.WorkEmail.ToLower() == searchEmail, null, cancellationToken);
        }

        if (employee is null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.EmployeeNotFound);

        var payslips = await payslipRepository.GetManyByConditionAsync(
            x => x.EmployeeId == employee.Id && x.Status == PayslipStatus.Published,
            query => query.Include(x => x.PayrollRun),
            cancellationToken);

        var periodIds = payslips.Select(x => x.PayrollRun.PayrollPeriodId).Distinct().ToList();

        var periods = await payrollPeriodRepository.GetManyByConditionAsync(
            x => periodIds.Contains(x.Id),
            query => query.OrderByDescending(p => p.StartDate),
            cancellationToken);

        return periods.Select(mapper.ToEssPayrollPeriodResponse).ToList();
    }
}
