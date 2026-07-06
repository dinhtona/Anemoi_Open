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

namespace Anemoi.Hr.Application.Cqrs.Commands.RecruitmentCommands.CreateRejectDecision;

public sealed class CreateRejectDecisionHandler(
    ISqlRepository<CandidateApplication> applicationRepository,
    ISqlRepository<HiringDecision> decisionRepository,
    IUnitOfWork unitOfWork,
    RecruitmentMapper mapper)
    : ICommandHandler<CreateRejectDecisionCommand, OneOf<HiringDecisionResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<HiringDecisionResponse, ErrorDetailResponse>> Handle(
        CreateRejectDecisionCommand request,
        CancellationToken cancellationToken)
    {
        var app = await applicationRepository.GetQueryable()
            .Include(x => x.Candidate)
            .Include(x => x.JobPosting)
            .FirstOrDefaultAsync(x => x.Id == request.CandidateApplicationId, cancellationToken);
        if (app is null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.ApplicationNotFound);
        if (CandidateApplicationStageCode.IsTerminal(app.CurrentStage))
            return HrErrorResponses.Create(HrBusinessErrorCodes.ApplicationInvalidStageTransition);

        var existing = await decisionRepository.ExistByConditionAsync(
            x => x.CandidateApplicationId == request.CandidateApplicationId, cancellationToken);
        if (existing)
            return HrErrorResponses.Create(HrBusinessErrorCodes.HiringDecisionAlreadyExists);

        if (!CandidateApplicationStageCode.IsValidTransition(app.CurrentStage, CandidateApplicationStageCode.Rejected))
            return HrErrorResponses.Create(HrBusinessErrorCodes.ApplicationInvalidStageTransition);

        var now = DateTime.UtcNow;
        var decision = HiringDecision.Create(
            new HiringDecisionId(IdGenerator.NextGuid()),
            request.CandidateApplicationId,
            HiringDecisionCode.Reject,
            request.DecidedBy,
            now,
            request.Notes);

        // Transition application to Rejected
        app.MoveToStage(CandidateApplicationStageCode.Rejected,
            new CandidateApplicationStageHistoryId(IdGenerator.NextGuid()),
            request.DecidedBy, now, "Rejected via decision");

        var createResult = await decisionRepository.CreateOneAsync(decision, cancellationToken);
        if (createResult.TryPickT1(out var exception, out _))
            return HrErrorResponses.FromSaveResult(exception, HrBusinessErrorCodes.SaveChangesFailed);

        var saveResult = await unitOfWork.SaveChangesAsync(cancellationToken);
        if (saveResult.TryPickT1(out var saveException, out _))
            return HrErrorResponses.FromSaveResult(saveException, HrBusinessErrorCodes.SaveChangesFailed);

        return mapper.ToResponse(decision);
    }
}
