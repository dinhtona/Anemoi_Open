using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Cqrs.Common.Dtos;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Domain.Separations;
using Microsoft.EntityFrameworkCore;
using OneOf;

namespace Anemoi.Hr.Application.Cqrs.Queries.SeparationQueries.GetSeparationById;

public sealed class GetSeparationByIdHandler(
    ISqlRepository<EmployeeSeparation> repository,
    EmployeeSeparationMapper mapper)
    : IQueryHandler<GetSeparationByIdQuery, OneOf<EmployeeSeparationDto, ErrorDetailResponse>>
{
    public async Task<OneOf<EmployeeSeparationDto, ErrorDetailResponse>> Handle(
        GetSeparationByIdQuery request, CancellationToken cancellationToken)
    {
        var separation = await repository.GetQueryable()
            .Include(x => x.Employee)
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        return separation is null
            ? HrErrorResponses.Create(HrBusinessErrorCodes.SeparationNotFound!)
            : mapper.ToDto(separation);
    }
}
