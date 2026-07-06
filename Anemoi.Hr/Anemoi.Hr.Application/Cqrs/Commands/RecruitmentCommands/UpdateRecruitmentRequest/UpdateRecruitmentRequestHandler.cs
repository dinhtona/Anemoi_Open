using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Commands;
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

namespace Anemoi.Hr.Application.Cqrs.Commands.RecruitmentCommands.UpdateRecruitmentRequest;

public sealed class UpdateRecruitmentRequestHandler(
    ISqlRepository<RecruitmentRequest> requestRepository,
    IUnitOfWork unitOfWork,
    RecruitmentMapper mapper)
    : ICommandHandler<UpdateRecruitmentRequestCommand, OneOf<RecruitmentRequestResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<RecruitmentRequestResponse, ErrorDetailResponse>> Handle(
        UpdateRecruitmentRequestCommand request,
        CancellationToken cancellationToken)
    {
        var id = new RecruitmentRequestId(Guid.Parse(request.Id));
        var recruitmentRequest = await requestRepository.GetQueryable()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (recruitmentRequest is null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.RecruitmentRequestNotFound);

        if (!recruitmentRequest.CanModify)
            return HrErrorResponses.Create(HrBusinessErrorCodes.RecruitmentRequestInvalidStatus);

        recruitmentRequest.DepartmentId = new DepartmentId(Guid.Parse(request.DepartmentId));
        recruitmentRequest.PositionId = new PositionId(Guid.Parse(request.PositionId));
        recruitmentRequest.RequestedHeadcount = request.RequestedHeadcount;
        recruitmentRequest.Reason = request.Reason;
        recruitmentRequest.PriorityCode = request.PriorityCode;
        recruitmentRequest.UpdatedAt = DateTime.UtcNow;

        var saveResult = await unitOfWork.SaveChangesAsync(cancellationToken);
        if (saveResult.TryPickT1(out var exception, out _))
            return HrErrorResponses.FromSaveResult(exception, HrBusinessErrorCodes.SaveChangesFailed);

        return mapper.ToResponse(recruitmentRequest);
    }
}
