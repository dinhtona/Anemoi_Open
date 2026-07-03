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

namespace Anemoi.Hr.Application.Cqrs.Commands.RecruitmentCommands.CreateRecruitmentRequest;

public sealed class CreateRecruitmentRequestHandler(
    ISqlRepository<RecruitmentRequest> requestRepository,
    IUnitOfWork unitOfWork,
    RecruitmentMapper mapper)
    : ICommandHandler<CreateRecruitmentRequestCommand, OneOf<RecruitmentRequestResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<RecruitmentRequestResponse, ErrorDetailResponse>> Handle(
        CreateRecruitmentRequestCommand request,
        CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var requestId = new RecruitmentRequestId(IdGenerator.NextGuid());

        var requestNumber = await GenerateRequestNumber(now, cancellationToken);

        var recruitmentRequest = RecruitmentRequest.Create(
            requestId,
            requestNumber,
            new DepartmentId(Guid.Parse(request.DepartmentId)),
            new PositionId(Guid.Parse(request.PositionId)),
            request.RequestedHeadcount,
            request.Reason,
            request.PriorityCode,
            request.CreatedBy,
            now);

        var createResult = await requestRepository.CreateOneAsync(recruitmentRequest, cancellationToken);
        if (createResult.TryPickT1(out var exception, out _))
            return HrErrorResponses.FromSaveResult(exception, HrBusinessErrorCodes.SaveChangesFailed);

        var saveResult = await unitOfWork.SaveChangesAsync(cancellationToken);
        if (saveResult.TryPickT1(out var saveException, out _))
            return HrErrorResponses.FromSaveResult(saveException, HrBusinessErrorCodes.SaveChangesFailed);

        return mapper.ToResponse(recruitmentRequest);
    }

    private async Task<string> GenerateRequestNumber(DateTime now, CancellationToken cancellationToken)
    {
        var prefix = $"RR-{now:yyyyMM}-";
        var existing = await requestRepository.GetQueryable()
            .Where(x => x.RequestNumber.StartsWith(prefix))
            .CountAsync(cancellationToken);
        return $"{prefix}{(existing + 1):D4}";
    }
}
