using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.Hr.Application.Responses;
using System;
using System.Collections.Generic;

namespace Anemoi.Hr.Application.Cqrs.Queries.RecruitmentQueries.GetCandidateSourceEffectiveness;

public sealed record GetCandidateSourceEffectivenessQuery(
    DateOnly? FromDate,
    DateOnly? ToDate) : IQuery<IReadOnlyCollection<CandidateSourceEffectivenessItem>>;
