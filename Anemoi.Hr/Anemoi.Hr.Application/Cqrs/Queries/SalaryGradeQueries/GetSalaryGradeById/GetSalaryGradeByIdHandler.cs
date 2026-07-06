using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Compensation;
using OneOf;

namespace Anemoi.Hr.Application.Cqrs.Queries.SalaryGradeQueries.GetSalaryGradeById;

public sealed class GetSalaryGradeByIdHandler(
    ISqlRepository<SalaryGrade> repository,
    CompensationMapper mapper)
    : IQueryHandler<GetSalaryGradeByIdQuery, OneOf<SalaryGradeResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<SalaryGradeResponse, ErrorDetailResponse>> Handle(
        GetSalaryGradeByIdQuery request,
        CancellationToken cancellationToken)
    {
        var salaryGrade = await repository.GetFirstByConditionAsync(
            x => x.Id == request.Id,
            null,
            cancellationToken);

        return salaryGrade is null
            ? HrErrorResponses.Create(HrBusinessErrorCodes.SalaryGradeNotFound)
            : mapper.ToSalaryGradeResponse(salaryGrade);
    }
}
