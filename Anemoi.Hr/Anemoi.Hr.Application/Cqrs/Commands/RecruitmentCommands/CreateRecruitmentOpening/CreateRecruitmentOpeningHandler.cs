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

namespace Anemoi.Hr.Application.Cqrs.Commands.RecruitmentCommands.CreateRecruitmentOpening;

public sealed class CreateRecruitmentOpeningHandler(
    ISqlRepository<RecruitmentRequest> requestRepository,
    ISqlRepository<RecruitmentOpening> openingRepository,
    IUnitOfWork unitOfWork,
    IPublishEndpoint publishEndpoint,
    RecruitmentMapper mapper)
    : ICommandHandler<CreateRecruitmentOpeningCommand, OneOf<RecruitmentOpeningResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<RecruitmentOpeningResponse, ErrorDetailResponse>> Handle(
        CreateRecruitmentOpeningCommand request,
        CancellationToken cancellationToken)
    {
        var requestId = new RecruitmentRequestId(Guid.Parse(request.RecruitmentRequestId));
        var recruitmentRequest = await requestRepository.GetQueryable()
            .FirstOrDefaultAsync(x => x.Id == requestId, cancellationToken);

        if (recruitmentRequest is null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.RecruitmentRequestNotFound);

        if (recruitmentRequest.Status != RecruitmentRequestStatusCode.Approved)
            return HrErrorResponses.Create(HrBusinessErrorCodes.RecruitmentRequestNotApproved);

        var now = DateTime.UtcNow;
        var openingId = new RecruitmentOpeningId(IdGenerator.NextGuid());

        var opening = new RecruitmentOpening
        {
            Id = openingId,
            RecruitmentRequestId = requestId,
            Code = request.Code,
            PlannedHeadcount = request.PlannedHeadcount,
            FilledHeadcount = 0,
            Status = "Open",
            OpenedAt = now,
            CreatedAt = now,
            UpdatedAt = now
        };

        var createResult = await openingRepository.CreateOneAsync(opening, cancellationToken);
        if (createResult.TryPickT1(out var createException, out _))
            return HrErrorResponses.FromSaveResult(createException, HrBusinessErrorCodes.SaveChangesFailed);

        // Publish integration event BEFORE SaveChanges (ADR-025)
        await publishEndpoint.Publish(new RecruitmentOpeningCreatedIntegrationEvent(
            openingId.Value.ToString(),
            request.RecruitmentRequestId,
            request.Code,
            request.PlannedHeadcount
        ), cancellationToken);

        var saveResult = await unitOfWork.SaveChangesAsync(cancellationToken);
        if (saveResult.TryPickT1(out var exception, out _))
            return HrErrorResponses.FromSaveResult(exception, HrBusinessErrorCodes.SaveChangesFailed);

        return mapper.ToResponse(opening);
    }
}
