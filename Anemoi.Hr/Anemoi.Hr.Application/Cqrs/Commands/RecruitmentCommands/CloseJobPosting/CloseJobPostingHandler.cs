using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Recruitment;
using Microsoft.EntityFrameworkCore;
using OneOf;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Anemoi.Hr.Application.Cqrs.Commands.RecruitmentCommands.CloseJobPosting;

public sealed class CloseJobPostingHandler(
    ISqlRepository<JobPosting> postingRepository,
    IUnitOfWork unitOfWork,
    RecruitmentMapper mapper)
    : ICommandHandler<CloseJobPostingCommand, OneOf<JobPostingResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<JobPostingResponse, ErrorDetailResponse>> Handle(
        CloseJobPostingCommand request,
        CancellationToken cancellationToken)
    {
        var posting = await postingRepository.GetQueryable()
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (posting is null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.JobPostingNotFound);

        if (!posting.Close(request.ClosedBy, DateTime.UtcNow))
            return HrErrorResponses.Create(HrBusinessErrorCodes.JobPostingInvalidStatus);

        var saveResult = await unitOfWork.SaveChangesAsync(cancellationToken);
        if (saveResult.TryPickT1(out var exception, out _))
            return HrErrorResponses.FromSaveResult(exception, HrBusinessErrorCodes.SaveChangesFailed);

        return mapper.ToResponse(posting);
    }
}
