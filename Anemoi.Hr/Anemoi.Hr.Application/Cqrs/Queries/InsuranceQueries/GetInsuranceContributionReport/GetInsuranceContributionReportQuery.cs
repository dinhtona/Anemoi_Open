using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.Hr.Application.Responses;
using System;
using System.Collections.Generic;

namespace Anemoi.Hr.Application.Cqrs.Queries.InsuranceQueries.GetInsuranceContributionReport;

public sealed record GetInsuranceContributionReportQuery(
    DateOnly PeriodStart,
    DateOnly PeriodEnd,
    string? CountryCode = null,
    string? InsuranceType = null) : IQuery<IReadOnlyCollection<InsuranceCalculationSnapshotResponse>>;
