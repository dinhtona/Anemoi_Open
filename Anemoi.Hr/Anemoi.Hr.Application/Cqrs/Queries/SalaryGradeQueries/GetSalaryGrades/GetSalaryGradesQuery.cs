using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.BuildingBlock.Application.Queries;
using Anemoi.Hr.Application.Responses;

namespace Anemoi.Hr.Application.Cqrs.Queries.SalaryGradeQueries.GetSalaryGrades;

public sealed record GetSalaryGradesQuery(string? SearchKey, bool? IsActive)
    : GetManyQuery, IQueryPaged<SalaryGradeResponse>;
