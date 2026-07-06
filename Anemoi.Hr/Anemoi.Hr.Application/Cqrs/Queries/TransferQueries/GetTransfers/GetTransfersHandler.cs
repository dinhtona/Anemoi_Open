using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Cqrs.Common.Dtos;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Domain.Transfers;
using Microsoft.EntityFrameworkCore;

namespace Anemoi.Hr.Application.Cqrs.Queries.TransferQueries.GetTransfers;

public sealed class GetTransfersHandler(
    ISqlRepository<EmployeeTransfer> repository,
    EmployeeTransferMapper mapper)
    : IQueryHandler<GetTransfersQuery, PaginationResponse<EmployeeTransferDto>>
{
    public async Task<PaginationResponse<EmployeeTransferDto>> Handle(
        GetTransfersQuery request, CancellationToken cancellationToken)
    {
        var query = repository.GetQueryable()
            .Include(x => x.Employee)
            .Include(x => x.SourceDepartment)
            .Include(x => x.TargetDepartment)
            .Include(x => x.SourcePosition)
            .Include(x => x.TargetPosition)
            .AsQueryable();

        query = query.Where(x =>
            (request.EmployeeId == null || x.EmployeeId == request.EmployeeId) &&
            (string.IsNullOrEmpty(request.StatusCode) || x.StatusCode == request.StatusCode));

        var total = await query.LongCountAsync(cancellationToken);

        var paged = query.OrderByDescending(x => x.CreatedAt);
        var skip = request.GetSkip();
        if (skip.HasValue)
            paged = (IOrderedQueryable<EmployeeTransfer>)paged.Skip(skip.Value);
        var take = request.GetTake();
        if (take.HasValue)
            paged = (IOrderedQueryable<EmployeeTransfer>)paged.Take(take.Value);

        var items = await paged.ToListAsync(cancellationToken);

        return new PaginationResponse<EmployeeTransferDto>(
            items.Select(mapper.ToDto).ToList(),
            total);
    }
}
