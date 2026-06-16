using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.BuildingBlock.Application.Helpers;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Recruitment;
using Anemoi.Hr.ModelIds.ModelIds;
using OneOf;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Anemoi.Hr.Application.Cqrs.Commands.RecruitmentCommands.CreateCandidate;

public sealed class CreateCandidateHandler(
    ISqlRepository<Candidate> candidateRepository,
    IUnitOfWork unitOfWork,
    RecruitmentMapper mapper)
    : ICommandHandler<CreateCandidateCommand, OneOf<CandidateResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<CandidateResponse, ErrorDetailResponse>> Handle(
        CreateCandidateCommand request,
        CancellationToken cancellationToken)
    {
        var emailExists = await candidateRepository.ExistByConditionAsync(
            x => x.Email == request.Email, cancellationToken);
        if (emailExists)
            return HrErrorResponses.Create(HrBusinessErrorCodes.CandidateEmailAlreadyExists);

        if (!string.IsNullOrWhiteSpace(request.PhoneNumber))
        {
            var phoneExists = await candidateRepository.ExistByConditionAsync(
                x => x.PhoneNumber == request.PhoneNumber, cancellationToken);
            if (phoneExists)
                return HrErrorResponses.Create(HrBusinessErrorCodes.CandidatePhoneAlreadyExists);
        }

        var codeExists = await candidateRepository.ExistByConditionAsync(
            x => x.CandidateCode == request.CandidateCode, cancellationToken);
        if (codeExists)
            return HrErrorResponses.Create(HrBusinessErrorCodes.CandidateCodeAlreadyExists);

        var now = DateTime.UtcNow;
        var candidate = new Candidate
        {
            Id = new CandidateId(IdGenerator.NextGuid()),
            CandidateCode = request.CandidateCode,
            FullName = request.FullName,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            DateOfBirth = request.DateOfBirth,
            Address = request.Address,
            ResumeUrl = request.ResumeUrl,
            Source = request.Source,
            Notes = request.Notes,
            CreatedBy = request.CreatedBy,
            CreatedAt = now,
            UpdatedBy = request.CreatedBy,
            UpdatedAt = now
        };

        var createResult = await candidateRepository.CreateOneAsync(candidate, cancellationToken);
        if (createResult.TryPickT1(out var exception, out _))
            return HrErrorResponses.FromSaveResult(exception, HrBusinessErrorCodes.SaveChangesFailed);

        var saveResult = await unitOfWork.SaveChangesAsync(cancellationToken);
        if (saveResult.TryPickT1(out var saveException, out _))
            return HrErrorResponses.FromSaveResult(saveException, HrBusinessErrorCodes.SaveChangesFailed);

        return mapper.ToResponse(candidate);
    }
}
