using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Helpers;
using Anemoi.Hr.ModelIds.ModelIds;
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

namespace Anemoi.Hr.Application.Cqrs.Commands.RecruitmentCommands.MoveCandidateApplicationToOffer;

public sealed class MoveCandidateApplicationToOfferHandler(
    ISqlRepository<CandidateApplication> applicationRepository,
    IUnitOfWork unitOfWork,
    RecruitmentMapper mapper)
    : ICommandHandler<MoveCandidateApplicationToOfferCommand, OneOf<CandidateApplicationResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<CandidateApplicationResponse, ErrorDetailResponse>> Handle(
        MoveCandidateApplicationToOfferCommand request,
        CancellationToken cancellationToken)
    {
        var app = await applicationRepository.GetQueryable()
            .Include(x => x.StageHistories)
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
        if (app is null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.ApplicationNotFound);

        if (!app.MoveToStage(CandidateApplicationStageCode.Offer, new CandidateApplicationStageHistoryId(IdGenerator.NextGuid()), request.ChangedBy, DateTime.UtcNow, request.Note))
            return HrErrorResponses.Create(HrBusinessErrorCodes.ApplicationInvalidStageTransition);

        var saveResult = await unitOfWork.SaveChangesAsync(cancellationToken);
        if (saveResult.TryPickT1(out var exception, out _))
            return HrErrorResponses.FromSaveResult(exception, HrBusinessErrorCodes.SaveChangesFailed);

        return mapper.ToResponse(app);
    }
}
