using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Employees;
using Anemoi.Hr.Domain.Payroll;
using OneOf;

namespace Anemoi.Hr.Application.Cqrs.Queries.EssQueries.GetMyPayslips;

public sealed class GetMyPayslipsHandler(
    ISqlRepository<Employee> employeeRepository,
    ISqlRepository<Payslip> payslipRepository,
    EssMapper mapper)
    : IQueryHandler<GetMyPayslipsQuery, OneOf<IReadOnlyCollection<EssPayslipResponse>, ErrorDetailResponse>>
{
    public async Task<OneOf<IReadOnlyCollection<EssPayslipResponse>, ErrorDetailResponse>> Handle(
        GetMyPayslipsQuery request, CancellationToken cancellationToken)
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
            query => query.OrderByDescending(p => p.GeneratedAt),
            cancellationToken);

        return payslips.Select(mapper.ToEssPayslipResponse).ToList();
    }
}
