using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.Hr.Application.Responses;
using System;
using System.Collections.Generic;

namespace Anemoi.Hr.Application.Cqrs.Queries.TaxationQueries.GetTaxCalculationSnapshots;

public sealed record GetTaxCalculationSnapshotsQuery(
    string? EmployeeId = null,
    string? SourceModule = null,
    Guid? SourceReferenceId = null) : IQuery<IReadOnlyCollection<TaxCalculationSnapshotResponse>>;
