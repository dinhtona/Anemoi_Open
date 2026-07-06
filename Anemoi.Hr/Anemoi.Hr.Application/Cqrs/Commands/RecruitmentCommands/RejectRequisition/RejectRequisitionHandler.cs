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

namespace Anemoi.Hr.Application.Cqrs.Commands.RecruitmentCommands.RejectRequisition;

public sealed class RejectRequisitionHandler(
    ISqlRepository<JobRequisition> requisitionRepository,
    IUnitOfWork unitOfWork,
    RecruitmentMapper mapper)
    : ICommandHandler<RejectRequisitionCommand, OneOf<JobRequisitionResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<JobRequisitionResponse, ErrorDetailResponse>> Handle(
        RejectRequisitionCommand request,
        CancellationToken cancellationToken)
    {
        var requisition = await requisitionRepository.GetQueryable()
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (requisition is null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.RequisitionNotFound);

        if (!requisition.Reject(request.RejectedBy, DateTime.UtcNow, request.Reason))
            return HrErrorResponses.Create(HrBusinessErrorCodes.RequisitionInvalidStatus);

        var saveResult = await unitOfWork.SaveChangesAsync(cancellationToken);
        if (saveResult.TryPickT1(out var exception, out _))
            return HrErrorResponses.FromSaveResult(exception, HrBusinessErrorCodes.RequisitionConcurrencyConflict);

        return mapper.ToResponse(requisition);
    }
}
