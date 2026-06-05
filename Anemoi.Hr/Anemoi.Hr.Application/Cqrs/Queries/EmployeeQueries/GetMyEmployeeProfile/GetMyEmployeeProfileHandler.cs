using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Employees;
using Microsoft.EntityFrameworkCore;
using OneOf;

namespace Anemoi.Hr.Application.Cqrs.Queries.EmployeeQueries.GetMyEmployeeProfile;

public sealed class GetMyEmployeeProfileHandler(ISqlRepository<Employee> repository, EmployeeMapper mapper)
    : IQueryHandler<GetMyEmployeeProfileQuery, OneOf<EmployeeResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<EmployeeResponse, ErrorDetailResponse>> Handle(GetMyEmployeeProfileQuery request,
        CancellationToken cancellationToken)
    {
        // TEMPORARY LIMITATION: Currently, the Employee entity does not have an Identity/User mapping field (e.g., UserId).
        // Once the mapping is added to the database model, we should resolve by request.UserId first.
        // For now, we fallback to finding the employee by WorkEmail matching the authenticated user's email.
        
        if (string.IsNullOrEmpty(request.Email))
        {
            return HrErrorResponses.Create(HrBusinessErrorCodes.EmployeeNotFound);
        }

        var employee = await repository.GetFirstByConditionAsync(
            x => x.WorkEmail == request.Email,
            q => q.Include(x => x.PrimaryDepartment)
                  .Include(x => x.PrimaryPosition)
                  .Include(x => x.DirectManager),
            cancellationToken);

        return employee is null
            ? HrErrorResponses.Create(HrBusinessErrorCodes.EmployeeNotFound)
            : mapper.ToEmployeeResponse(employee);
    }
}
