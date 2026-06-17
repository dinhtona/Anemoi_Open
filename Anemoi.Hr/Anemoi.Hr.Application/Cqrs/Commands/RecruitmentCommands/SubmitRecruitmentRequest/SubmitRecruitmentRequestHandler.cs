using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.BuildingBlock.Application.Helpers;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Contract.Hr.Events;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Recruitment;
using Anemoi.Hr.ModelIds.ModelIds;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using OneOf;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Anemoi.Hr.Application.Cqrs.Commands.RecruitmentCommands.SubmitRecruitmentRequest;

public sealed class SubmitRecruitmentRequestHandler(
    ISqlRepository<RecruitmentRequest> requestRepository,
    ISqlRepository<RecruitmentRequestHistory> historyRepository,
    IUnitOfWork unitOfWork,
    IPublishEndpoint publishEndpoint,
    RecruitmentMapper mapper)
    : ICommandHandler<SubmitRecruitmentRequestCommand, OneOf<RecruitmentRequestResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<RecruitmentRequestResponse, ErrorDetailResponse>> Handle(
        SubmitRecruitmentRequestCommand request,
        CancellationToken cancellationToken)
    {
        var id = new RecruitmentRequestId(Guid.Parse(request.Id));
        var recruitmentRequest = await requestRepository.GetQueryable()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (recruitmentRequest is null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.RecruitmentRequestNotFound);

        if (!recruitmentRequest.CanSubmit)
            return HrErrorResponses.Create(HrBusinessErrorCodes.RecruitmentRequestAlreadySubmitted);

        var now = DateTime.UtcNow;
        var oldStatus = recruitmentRequest.Status;
        recruitmentRequest.Submit(request.SubmittedBy, now);

        var history = new RecruitmentRequestHistory
        {
            Id = new RecruitmentRequestHistoryId(IdGenerator.NextGuid()),
            RecruitmentRequestId = recruitmentRequest.Id,
            ActionCode = "Submitted",
            OldStatus = oldStatus,
            NewStatus = recruitmentRequest.Status,
            PerformedBy = request.SubmittedBy,
            PerformedAt = now
        };
        var historyResult = await historyRepository.CreateOneAsync(history, cancellationToken);
        if (historyResult.TryPickT1(out var histException, out _))
            return HrErrorResponses.FromSaveResult(histException, HrBusinessErrorCodes.SaveChangesFailed);

        await publishEndpoint.Publish(new RecruitmentRequestSubmittedIntegrationEvent(
            recruitmentRequest.Id.Value.ToString(),
            recruitmentRequest.RequestNumber,
            request.SubmittedBy,
            request.SubmittedBy,
            recruitmentRequest.DepartmentId.Value.ToString(),
            recruitmentRequest.PositionId.Value.ToString(),
            recruitmentRequest.RequestedHeadcount
        ), cancellationToken);

        var saveResult = await unitOfWork.SaveChangesAsync(cancellationToken);
        if (saveResult.TryPickT1(out var exception, out _))
            return HrErrorResponses.FromSaveResult(exception, HrBusinessErrorCodes.SaveChangesFailed);

        return mapper.ToResponse(recruitmentRequest);
    }
}
