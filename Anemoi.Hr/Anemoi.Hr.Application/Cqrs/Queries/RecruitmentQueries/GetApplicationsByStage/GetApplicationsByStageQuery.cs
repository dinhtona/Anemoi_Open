using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.Hr.Application.Responses;
using System;
using System.Collections.Generic;

namespace Anemoi.Hr.Application.Cqrs.Queries.RecruitmentQueries.GetApplicationsByStage;

public sealed record GetApplicationsByStageQuery(
    DateOnly? FromDate,
    DateOnly? ToDate) : IQuery<IReadOnlyCollection<ApplicationsByStageItem>>;
