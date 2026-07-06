using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Contracts;
using Anemoi.Hr.Domain.Employees;
using OneOf;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Anemoi.Hr.Application.Cqrs.Queries.ContractQueries.GetEmployeeContracts;

public sealed class GetEmployeeContractsHandler(
    ISqlRepository<Employee> employeeRepository,
    ISqlRepository<EmployeeContract> contractRepository,
    EmployeeContractMapper mapper)
    : IQueryHandler<GetEmployeeContractsQuery, OneOf<IReadOnlyCollection<EmployeeContractResponse>, ErrorDetailResponse>>
{
    public async Task<OneOf<IReadOnlyCollection<EmployeeContractResponse>, ErrorDetailResponse>> Handle(
        GetEmployeeContractsQuery request,
        CancellationToken cancellationToken)
    {
        // 1. Employee existence check
        var employeeExists = await employeeRepository.ExistByConditionAsync(x => x.Id == request.EmployeeId, cancellationToken);
        if (!employeeExists)
            return HrErrorResponses.Create(HrBusinessErrorCodes.EmployeeNotFound);

        // 2. Fetch contracts, ordered by StartDate descending
        var contracts = await contractRepository.GetManyByConditionAsync(
            x => x.EmployeeId == request.EmployeeId,
            q => q.OrderByDescending(x => x.StartDate),
            cancellationToken);

        return OneOf<IReadOnlyCollection<EmployeeContractResponse>, ErrorDetailResponse>.FromT0(mapper.ToResponses(contracts));
    }
}
