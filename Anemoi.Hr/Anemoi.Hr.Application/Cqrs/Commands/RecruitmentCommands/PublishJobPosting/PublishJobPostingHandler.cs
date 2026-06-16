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

namespace Anemoi.Hr.Application.Cqrs.Commands.RecruitmentCommands.PublishJobPosting;

public sealed class PublishJobPostingHandler(
    ISqlRepository<JobPosting> postingRepository,
    IUnitOfWork unitOfWork,
    RecruitmentMapper mapper)
    : ICommandHandler<PublishJobPostingCommand, OneOf<JobPostingResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<JobPostingResponse, ErrorDetailResponse>> Handle(
        PublishJobPostingCommand request,
        CancellationToken cancellationToken)
    {
        var posting = await postingRepository.GetQueryable()
            .Include(x => x.JobRequisition)
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (posting is null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.JobPostingNotFound);

        if (posting.JobRequisition is not null && !posting.JobRequisition.CanCreatePosting)
            return HrErrorResponses.Create(HrBusinessErrorCodes.RequisitionNotApprovedForPosting);

        if (!posting.Publish(request.PublishedBy, DateTime.UtcNow))
            return HrErrorResponses.Create(HrBusinessErrorCodes.JobPostingInvalidStatus);

        var saveResult = await unitOfWork.SaveChangesAsync(cancellationToken);
        if (saveResult.TryPickT1(out var exception, out _))
            return HrErrorResponses.FromSaveResult(exception, HrBusinessErrorCodes.SaveChangesFailed);

        return mapper.ToResponse(posting);
    }
}
