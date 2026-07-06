using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;

namespace Anemoi.Hr.Application.Cqrs.Queries.ContractQueries.GetContractDetail;

public sealed record GetContractDetailQuery(EmployeeContractId Id)
    : IQueryOne<EmployeeContractDetailResponse>;
