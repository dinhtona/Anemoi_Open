using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.ModelIds.ModelIds;
using System;

namespace Anemoi.Hr.Application.Cqrs.Queries.CompensationQueries.GetCompensationSnapshot;

public sealed record GetCompensationSnapshotQuery(
    EmployeeId EmployeeId,
    DateOnly ReferenceDate) : IQuery<CompensationSnapshotResponse>;
