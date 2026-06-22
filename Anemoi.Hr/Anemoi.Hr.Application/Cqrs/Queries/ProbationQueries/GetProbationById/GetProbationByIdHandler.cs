using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Cqrs.Common.Dtos;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Domain.Probation;
using Microsoft.EntityFrameworkCore;
using OneOf;

namespace Anemoi.Hr.Application.Cqrs.Queries.ProbationQueries.GetProbationById;

public sealed class GetProbationByIdHandler(
    ISqlRepository<ProbationRecord> repository,
    ProbationRecordMapper mapper)
    : IQueryHandler<GetProbationByIdQuery, OneOf<ProbationRecordDto, ErrorDetailResponse>>
{
    public async Task<OneOf<ProbationRecordDto, ErrorDetailResponse>> Handle(
        GetProbationByIdQuery request, CancellationToken cancellationToken)
    {
        var record = await repository.GetQueryable()
            .Include(x => x.Employee)
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        return record is null
            ? HrErrorResponses.Create(HrBusinessErrorCodes.ProbationRecordNotFound!)
            : mapper.ToDto(record);
    }
}
