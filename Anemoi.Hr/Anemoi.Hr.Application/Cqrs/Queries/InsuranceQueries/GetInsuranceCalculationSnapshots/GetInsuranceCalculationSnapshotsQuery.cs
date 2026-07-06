using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.Hr.Application.Responses;
using System.Collections.Generic;

namespace Anemoi.Hr.Application.Cqrs.Queries.InsuranceQueries.GetInsuranceCalculationSnapshots;

public sealed record GetInsuranceCalculationSnapshotsQuery(
    string? EmployeeId = null,
    string? InsuranceType = null,
    string? SourceModule = null,
    string? SourceReferenceId = null) : IQuery<IReadOnlyCollection<InsuranceCalculationSnapshotResponse>>;
