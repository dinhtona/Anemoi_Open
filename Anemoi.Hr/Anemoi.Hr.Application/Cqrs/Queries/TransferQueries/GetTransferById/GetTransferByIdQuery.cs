using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.Hr.Application.Cqrs.Common.Dtos;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Application.Cqrs.Queries.TransferQueries.GetTransferById;

public sealed record GetTransferByIdQuery(EmployeeTransferId Id) : IQueryOne<EmployeeTransferDto>;
