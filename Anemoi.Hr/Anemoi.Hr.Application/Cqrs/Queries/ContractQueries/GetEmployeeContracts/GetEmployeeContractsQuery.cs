using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;
using System.Collections.Generic;

namespace Anemoi.Hr.Application.Cqrs.Queries.ContractQueries.GetEmployeeContracts;

public sealed record GetEmployeeContractsQuery(EmployeeId EmployeeId)
    : IQueryOne<IReadOnlyCollection<EmployeeContractResponse>>;
