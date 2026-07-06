using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.Hr.Application.Responses;
using System;
using System.Collections.Generic;

namespace Anemoi.Hr.Application.Cqrs.Queries.RecruitmentQueries.GetHiringByDepartment;

public sealed record GetHiringByDepartmentQuery(
    DateOnly? FromDate,
    DateOnly? ToDate) : IQuery<IReadOnlyCollection<HiringByDepartmentItem>>;
