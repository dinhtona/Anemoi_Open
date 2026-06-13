using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Employees;
using Anemoi.Hr.Domain.Leaves;
using Microsoft.EntityFrameworkCore;
using OneOf;

namespace Anemoi.Hr.Application.Cqrs.Queries.EssQueries.GetMyLeaveBalances;

public sealed class GetMyLeaveBalancesHandler(
    ISqlRepository<Employee> employeeRepository,
    ISqlRepository<LeaveBalance> leaveBalanceRepository,
    EssMapper mapper)
    : IQueryHandler<GetMyLeaveBalancesQuery, OneOf<IReadOnlyCollection<EssLeaveBalanceResponse>, ErrorDetailResponse>>
{
    public async Task<OneOf<IReadOnlyCollection<EssLeaveBalanceResponse>, ErrorDetailResponse>> Handle(
        GetMyLeaveBalancesQuery request, CancellationToken cancellationToken)
    {
        var employee = await ResolveEmployeeAsync(request, cancellationToken);
        if (employee is null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.EmployeeNotFound);

        var balances = await leaveBalanceRepository.GetManyByConditionAsync(
            x => x.EmployeeId == employee.Id,
            query => query.Include(x => x.LeavePolicy),
            cancellationToken);

        return balances.Select(mapper.ToEssLeaveBalanceResponse).ToList();
    }

    private async Task<Employee> ResolveEmployeeAsync(GetMyLeaveBalancesQuery request,
        CancellationToken cancellationToken)
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

        return employee;
    }
}
