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

namespace Anemoi.Hr.Application.Cqrs.Commands.RecruitmentCommands.UpdateCandidate;

public sealed class UpdateCandidateHandler(
    ISqlRepository<Candidate> candidateRepository,
    IUnitOfWork unitOfWork,
    RecruitmentMapper mapper)
    : ICommandHandler<UpdateCandidateCommand, OneOf<CandidateResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<CandidateResponse, ErrorDetailResponse>> Handle(
        UpdateCandidateCommand request,
        CancellationToken cancellationToken)
    {
        var candidate = await candidateRepository.GetQueryable()
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (candidate is null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.CandidateNotFound);

        var emailConflict = await candidateRepository.ExistByConditionAsync(
            x => x.Email == request.Email && x.Id != request.Id, cancellationToken);
        if (emailConflict)
            return HrErrorResponses.Create(HrBusinessErrorCodes.CandidateEmailAlreadyExists);

        if (!string.IsNullOrWhiteSpace(request.PhoneNumber))
        {
            var phoneConflict = await candidateRepository.ExistByConditionAsync(
                x => x.PhoneNumber == request.PhoneNumber && x.Id != request.Id, cancellationToken);
            if (phoneConflict)
                return HrErrorResponses.Create(HrBusinessErrorCodes.CandidatePhoneAlreadyExists);
        }

        if (!candidate.UpdateProfile(request.FullName, request.Email, request.PhoneNumber,
                request.DateOfBirth, request.Address, request.ResumeUrl, request.Notes))
            return HrErrorResponses.Create(HrBusinessErrorCodes.ValCandidateNameRequired);

        candidate.UpdatedBy = request.UpdatedBy;
        candidate.UpdatedAt = DateTime.UtcNow;

        var saveResult = await unitOfWork.SaveChangesAsync(cancellationToken);
        if (saveResult.TryPickT1(out var exception, out _))
            return HrErrorResponses.FromSaveResult(exception, HrBusinessErrorCodes.SaveChangesFailed);

        return mapper.ToResponse(candidate);
    }
}
