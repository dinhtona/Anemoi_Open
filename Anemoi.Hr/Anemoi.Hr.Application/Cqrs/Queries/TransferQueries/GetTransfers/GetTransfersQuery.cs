using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.BuildingBlock.Application.Queries;
using Anemoi.Hr.Application.Cqrs.Common.Dtos;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Application.Cqrs.Queries.TransferQueries.GetTransfers;

public sealed record GetTransfersQuery(
    EmployeeId? EmployeeId,
    string? StatusCode) : GetManyQuery, IQueryPaged<EmployeeTransferDto>;
