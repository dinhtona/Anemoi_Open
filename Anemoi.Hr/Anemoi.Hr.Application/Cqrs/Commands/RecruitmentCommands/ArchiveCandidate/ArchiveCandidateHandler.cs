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

namespace Anemoi.Hr.Application.Cqrs.Commands.RecruitmentCommands.ArchiveCandidate;

public sealed class ArchiveCandidateHandler(
    ISqlRepository<Candidate> candidateRepository,
    IUnitOfWork unitOfWork,
    RecruitmentMapper mapper)
    : ICommandHandler<ArchiveCandidateCommand, OneOf<CandidateResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<CandidateResponse, ErrorDetailResponse>> Handle(
        ArchiveCandidateCommand request,
        CancellationToken cancellationToken)
    {
        var candidate = await candidateRepository.GetQueryable()
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (candidate is null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.CandidateNotFound);

        if (!candidate.Archive(request.UpdatedBy, DateTime.UtcNow))
            return HrErrorResponses.Create(HrBusinessErrorCodes.CandidateNotFound);

        var saveResult = await unitOfWork.SaveChangesAsync(cancellationToken);
        if (saveResult.TryPickT1(out var exception, out _))
            return HrErrorResponses.FromSaveResult(exception, HrBusinessErrorCodes.SaveChangesFailed);

        return mapper.ToResponse(candidate);
    }
}
