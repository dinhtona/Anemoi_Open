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

namespace Anemoi.Hr.Application.Cqrs.Commands.RecruitmentCommands.RejectRecruitmentRequest;

public sealed class RejectRecruitmentRequestHandler(
    ISqlRepository<RecruitmentRequest> requestRepository,
    ISqlRepository<RecruitmentRequestHistory> historyRepository,
    IUnitOfWork unitOfWork,
    IPublishEndpoint publishEndpoint,
    RecruitmentMapper mapper)
    : ICommandHandler<RejectRecruitmentRequestCommand, OneOf<RecruitmentRequestResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<RecruitmentRequestResponse, ErrorDetailResponse>> Handle(
        RejectRecruitmentRequestCommand request,
        CancellationToken cancellationToken)
    {
        var id = new RecruitmentRequestId(Guid.Parse(request.Id));
        var recruitmentRequest = await requestRepository.GetQueryable()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (recruitmentRequest is null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.RecruitmentRequestNotFound);

        if (!recruitmentRequest.CanReject)
            return HrErrorResponses.Create(HrBusinessErrorCodes.RecruitmentRequestInvalidStatus);

        var now = DateTime.UtcNow;
        var oldStatus = recruitmentRequest.Status;
        recruitmentRequest.Reject(request.RejectedBy, now, request.Comment);

        var history = new RecruitmentRequestHistory
        {
            Id = new RecruitmentRequestHistoryId(IdGenerator.NextGuid()),
            RecruitmentRequestId = recruitmentRequest.Id,
            ActionCode = "Rejected",
            OldStatus = oldStatus,
            NewStatus = recruitmentRequest.Status,
            Comment = request.Comment,
            PerformedBy = request.RejectedBy,
            PerformedAt = now
        };
        var historyResult = await historyRepository.CreateOneAsync(history, cancellationToken);
        if (historyResult.TryPickT1(out var histException, out _))
            return HrErrorResponses.FromSaveResult(histException, HrBusinessErrorCodes.SaveChangesFailed);

        await publishEndpoint.Publish(new RecruitmentRequestRejectedIntegrationEvent(
            recruitmentRequest.Id.Value.ToString(),
            recruitmentRequest.RequestNumber,
            request.RejectedBy,
            request.Comment
        ), cancellationToken);

        var saveResult = await unitOfWork.SaveChangesAsync(cancellationToken);
        if (saveResult.TryPickT1(out var exception, out _))
            return HrErrorResponses.FromSaveResult(exception, HrBusinessErrorCodes.SaveChangesFailed);

        return mapper.ToResponse(recruitmentRequest);
    }
}
