using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Departments;
using OneOf;

namespace Anemoi.Hr.Application.Cqrs.Queries.DepartmentQueries.GetDepartmentById;

public sealed class GetDepartmentByIdHandler(
    ISqlRepository<Department> repository,
    EmployeeMapper mapper)
    : IQueryHandler<GetDepartmentByIdQuery, OneOf<DepartmentResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<DepartmentResponse, ErrorDetailResponse>> Handle(
        GetDepartmentByIdQuery request,
        CancellationToken cancellationToken)
    {
        var department = await repository.GetFirstByConditionAsync(
            x => x.Id == request.Id,
            null,
            cancellationToken);

        return department is null
            ? HrErrorResponses.Create(HrBusinessErrorCodes.DepartmentNotFound)
            : mapper.ToDepartmentResponse(department);
    }
}
