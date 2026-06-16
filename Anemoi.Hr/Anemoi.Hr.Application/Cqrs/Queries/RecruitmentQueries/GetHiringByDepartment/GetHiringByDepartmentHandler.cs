using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Departments;
using Anemoi.Hr.Domain.Recruitment;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Anemoi.Hr.Application.Cqrs.Queries.RecruitmentQueries.GetHiringByDepartment;

public sealed class GetHiringByDepartmentHandler(
    ISqlRepository<JobRequisition> requisitionRepository,
    ISqlRepository<CandidateApplication> applicationRepository,
    ISqlRepository<Department> departmentRepository)
    : IQueryHandler<GetHiringByDepartmentQuery, IReadOnlyCollection<HiringByDepartmentItem>>
{
    public async Task<IReadOnlyCollection<HiringByDepartmentItem>> Handle(
        GetHiringByDepartmentQuery request,
        CancellationToken cancellationToken)
    {
        var departments = await departmentRepository.GetManyByConditionAsync(token: cancellationToken);

        var result = new List<HiringByDepartmentItem>();
        foreach (var dept in departments)
        {
            var requisitionCount = await requisitionRepository.CountByConditionAsync(
                x => x.DepartmentId == dept.Id, token: cancellationToken);

            var appQuery = applicationRepository.GetQueryable()
                .Where(x => x.JobPosting.JobRequisition.DepartmentId == dept.Id);
            if (request.FromDate.HasValue)
                appQuery = appQuery.Where(x => x.AppliedAt >= request.FromDate.Value.ToDateTime(new TimeOnly(0, 0)));
            if (request.ToDate.HasValue)
                appQuery = appQuery.Where(x => x.AppliedAt <= request.ToDate.Value.ToDateTime(new TimeOnly(23, 59)));

            var applicationCount = await appQuery.CountAsync(cancellationToken);
            var hireCount = await appQuery
                .Where(x => x.CurrentStage == CandidateApplicationStageCode.Hired)
                .CountAsync(cancellationToken);

            result.Add(new HiringByDepartmentItem(
                dept.Id.Value.ToString(),
                dept.Name,
                (int)requisitionCount,
                applicationCount,
                hireCount,
                0
            ));
        }

        return result;
    }
}
