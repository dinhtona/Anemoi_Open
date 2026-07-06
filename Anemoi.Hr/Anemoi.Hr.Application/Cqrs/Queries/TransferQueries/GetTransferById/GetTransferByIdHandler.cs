using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Cqrs.Common.Dtos;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Domain.Transfers;
using Microsoft.EntityFrameworkCore;
using OneOf;

namespace Anemoi.Hr.Application.Cqrs.Queries.TransferQueries.GetTransferById;

public sealed class GetTransferByIdHandler(
    ISqlRepository<EmployeeTransfer> repository,
    EmployeeTransferMapper mapper)
    : IQueryHandler<GetTransferByIdQuery, OneOf<EmployeeTransferDto, ErrorDetailResponse>>
{
    public async Task<OneOf<EmployeeTransferDto, ErrorDetailResponse>> Handle(
        GetTransferByIdQuery request, CancellationToken cancellationToken)
    {
        var transfer = await repository.GetQueryable()
            .Include(x => x.Employee)
            .Include(x => x.SourceDepartment)
            .Include(x => x.TargetDepartment)
            .Include(x => x.SourcePosition)
            .Include(x => x.TargetPosition)
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        return transfer is null
            ? HrErrorResponses.Create(HrBusinessErrorCodes.TransferNotFound!)
            : mapper.ToDto(transfer);
    }
}
