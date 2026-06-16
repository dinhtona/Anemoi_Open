using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Employees;
using Anemoi.Hr.Domain.Onboarding;
using OneOf;
using System.Threading;
using System.Threading.Tasks;

namespace Anemoi.Hr.Application.Cqrs.Queries.OnboardingQueries.GetEmployeeOnboarding;

public sealed class GetEmployeeOnboardingHandler(
    ISqlRepository<OnboardingInstance> instanceRepository,
    ISqlRepository<Employee> employeeRepository,
    OnboardingMapper mapper)
    : IQueryHandler<GetEmployeeOnboardingQuery, OneOf<OnboardingInstanceResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<OnboardingInstanceResponse, ErrorDetailResponse>> Handle(
        GetEmployeeOnboardingQuery request, CancellationToken cancellationToken)
    {
        var instance = await instanceRepository.GetFirstByConditionAsync(
            x => x.EmployeeId == request.EmployeeId && x.Status == OnboardingInstanceStatusCode.InProgress,
            null, cancellationToken);

        if (instance is null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.HrOnboardingInstanceNotFound);

        var response = mapper.ToResponse(instance);

        var employee = await employeeRepository.GetFirstByConditionAsync(
            x => x.Id == request.EmployeeId,
            null, cancellationToken);

        if (employee is not null)
        {
            response.EmployeeName = employee.FullName;
            response.EmployeeCode = employee.EmployeeCode;
        }

        return response;
    }
}
