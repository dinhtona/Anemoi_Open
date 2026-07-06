using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.BuildingBlock.Application.Helpers;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Recruitment;
using Anemoi.Hr.ModelIds.ModelIds;
using Microsoft.EntityFrameworkCore;
using OneOf;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Anemoi.Hr.Application.Cqrs.Commands.RecruitmentCommands.CreateJobPosting;

public sealed class CreateJobPostingHandler(
    ISqlRepository<JobRequisition> requisitionRepository,
    ISqlRepository<JobPosting> postingRepository,
    IUnitOfWork unitOfWork,
    RecruitmentMapper mapper)
    : ICommandHandler<CreateJobPostingCommand, OneOf<JobPostingResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<JobPostingResponse, ErrorDetailResponse>> Handle(
        CreateJobPostingCommand request,
        CancellationToken cancellationToken)
    {
        var requisition = await requisitionRepository.GetQueryable()
            .FirstOrDefaultAsync(x => x.Id == request.JobRequisitionId, cancellationToken);

        if (requisition is null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.RequisitionNotFound);

        if (!requisition.CanCreatePosting)
            return HrErrorResponses.Create(HrBusinessErrorCodes.RequisitionNotApprovedForPosting);

        if (request.PublishDate > request.ExpiryDate)
            return HrErrorResponses.Create(HrBusinessErrorCodes.JobPostingDateRangeInvalid);

        var now = DateTime.UtcNow;
        var posting = new JobPosting
        {
            Id = new JobPostingId(IdGenerator.NextGuid()),
            JobRequisitionId = request.JobRequisitionId,
            PostingTitle = request.PostingTitle,
            PostingDescription = request.PostingDescription,
            PublishDate = request.PublishDate,
            ExpiryDate = request.ExpiryDate,
            CreatedBy = request.CreatedBy,
            CreatedAt = now,
            UpdatedBy = request.CreatedBy,
            UpdatedAt = now
        };

        var createResult = await postingRepository.CreateOneAsync(posting, cancellationToken);
        if (createResult.TryPickT1(out var exception, out _))
            return HrErrorResponses.FromSaveResult(exception, HrBusinessErrorCodes.SaveChangesFailed);

        var saveResult = await unitOfWork.SaveChangesAsync(cancellationToken);
        if (saveResult.TryPickT1(out var saveException, out _))
            return HrErrorResponses.FromSaveResult(saveException, HrBusinessErrorCodes.SaveChangesFailed);

        return mapper.ToResponse(posting);
    }
}
