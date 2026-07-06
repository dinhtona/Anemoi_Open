using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Employees;
using Microsoft.EntityFrameworkCore;
using OneOf;

namespace Anemoi.Hr.Application.Cqrs.Queries.EssQueries.GetMyProfile;

public sealed class GetMyProfileHandler(ISqlRepository<Employee> repository, EssMapper mapper)
    : IQueryHandler<GetMyProfileQuery, OneOf<EssEmployeeProfileResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<EssEmployeeProfileResponse, ErrorDetailResponse>> Handle(GetMyProfileQuery request,
        CancellationToken cancellationToken)
    {
        Employee employee = null;
        if (Guid.TryParse(request.UserId, out var identityUserId))
        {
            employee = await repository.GetFirstByConditionAsync(
                x => x.IdentityUserId == identityUserId,
                IncludeProfileRelations,
                cancellationToken);
        }

        if (employee is null && !string.IsNullOrEmpty(request.Email))
        {
            var searchEmail = request.Email.ToLower();
            employee = await repository.GetFirstByConditionAsync(
                x => x.WorkEmail != null && x.WorkEmail.ToLower() == searchEmail,
                IncludeProfileRelations,
                cancellationToken);
        }

        return employee is null
            ? HrErrorResponses.Create(HrBusinessErrorCodes.EmployeeNotFound)
            : mapper.ToEssEmployeeProfileResponse(employee);
    }

    private static IQueryable<Employee> IncludeProfileRelations(IQueryable<Employee> query) =>
        query.Include(x => x.PrimaryDepartment)
            .Include(x => x.PrimaryPosition)
            .Include(x => x.DirectManager);
}
