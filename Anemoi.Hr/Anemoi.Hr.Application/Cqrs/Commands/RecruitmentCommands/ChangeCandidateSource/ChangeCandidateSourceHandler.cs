using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Recruitment;
using Microsoft.EntityFrameworkCore;
using OneOf;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Anemoi.Hr.Application.Cqrs.Commands.RecruitmentCommands.ChangeCandidateSource;

public sealed class ChangeCandidateSourceHandler(
    ISqlRepository<Candidate> candidateRepository,
    IUnitOfWork unitOfWork,
    RecruitmentMapper mapper)
    : ICommandHandler<ChangeCandidateSourceCommand, OneOf<CandidateResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<CandidateResponse, ErrorDetailResponse>> Handle(
        ChangeCandidateSourceCommand request,
        CancellationToken cancellationToken)
    {
        var candidate = await candidateRepository.GetQueryable()
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (candidate is null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.CandidateNotFound);

        candidate.ChangeSource(request.Source, request.UpdatedBy);

        var saveResult = await unitOfWork.SaveChangesAsync(cancellationToken);
        if (saveResult.TryPickT1(out var exception, out _))
            return HrErrorResponses.FromSaveResult(exception, HrBusinessErrorCodes.SaveChangesFailed);

        return mapper.ToResponse(candidate);
    }
}
