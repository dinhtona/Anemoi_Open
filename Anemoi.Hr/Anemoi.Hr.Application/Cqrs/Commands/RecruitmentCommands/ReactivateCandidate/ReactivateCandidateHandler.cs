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

namespace Anemoi.Hr.Application.Cqrs.Commands.RecruitmentCommands.ReactivateCandidate;

public sealed class ReactivateCandidateHandler(
    ISqlRepository<Candidate> candidateRepository,
    IUnitOfWork unitOfWork,
    RecruitmentMapper mapper)
    : ICommandHandler<ReactivateCandidateCommand, OneOf<CandidateResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<CandidateResponse, ErrorDetailResponse>> Handle(
        ReactivateCandidateCommand request,
        CancellationToken cancellationToken)
    {
        var candidate = await candidateRepository.GetQueryable()
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (candidate is null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.CandidateNotFound);

        if (candidate.Status == CandidateStatusCode.Blacklisted)
            return HrErrorResponses.Create(HrBusinessErrorCodes.CandidateBlacklisted);

        if (!candidate.Reactivate(request.UpdatedBy, DateTime.UtcNow))
            return HrErrorResponses.Create(HrBusinessErrorCodes.CandidateNotFound);

        var saveResult = await unitOfWork.SaveChangesAsync(cancellationToken);
        if (saveResult.TryPickT1(out var exception, out _))
            return HrErrorResponses.FromSaveResult(exception, HrBusinessErrorCodes.SaveChangesFailed);

        return mapper.ToResponse(candidate);
    }
}
