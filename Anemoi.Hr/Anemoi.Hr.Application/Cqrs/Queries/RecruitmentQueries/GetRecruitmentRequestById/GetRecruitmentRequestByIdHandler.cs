using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Queries;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Recruitment;
using Anemoi.Hr.ModelIds.ModelIds;
using Microsoft.EntityFrameworkCore;
using OneOf;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Anemoi.Hr.Application.Cqrs.Queries.RecruitmentQueries.GetRecruitmentRequestById;

public sealed class GetRecruitmentRequestByIdHandler(
    ISqlRepository<RecruitmentRequest> requestRepository,
    RecruitmentMapper mapper)
    : IQueryHandler<GetRecruitmentRequestByIdQuery, OneOf<RecruitmentRequestResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<RecruitmentRequestResponse, ErrorDetailResponse>> Handle(
        GetRecruitmentRequestByIdQuery request,
        CancellationToken cancellationToken)
    {
        var id = new RecruitmentRequestId(Guid.Parse(request.Id));
        var recruitmentRequest = await requestRepository.GetQueryable()
            .Include(x => x.Department)
            .Include(x => x.Position)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (recruitmentRequest is null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.RecruitmentRequestNotFound);

        return mapper.ToResponse(recruitmentRequest);
    }
}
