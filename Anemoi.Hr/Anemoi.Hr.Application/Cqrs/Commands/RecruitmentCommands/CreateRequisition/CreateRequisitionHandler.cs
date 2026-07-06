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

namespace Anemoi.Hr.Application.Cqrs.Commands.RecruitmentCommands.CreateRequisition;

public sealed class CreateRequisitionHandler(
    ISqlRepository<JobRequisition> requisitionRepository,
    IUnitOfWork unitOfWork,
    RecruitmentMapper mapper)
    : ICommandHandler<CreateRequisitionCommand, OneOf<JobRequisitionResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<JobRequisitionResponse, ErrorDetailResponse>> Handle(
        CreateRequisitionCommand request,
        CancellationToken cancellationToken)
    {
        var exists = await requisitionRepository.ExistByConditionAsync(
            x => x.RequisitionCode == request.RequisitionCode, cancellationToken);
        if (exists)
            return HrErrorResponses.Create(HrBusinessErrorCodes.RequisitionCodeAlreadyExists);

        var now = DateTime.UtcNow;
        var requisition = new JobRequisition
        {
            Id = new JobRequisitionId(IdGenerator.NextGuid()),
            RequisitionCode = request.RequisitionCode,
            Title = request.Title,
            DepartmentId = request.DepartmentId,
            PositionId = request.PositionId,
            Headcount = request.Headcount,
            EmploymentType = request.EmploymentType,
            RequestedBy = request.CreatedBy,
            OpenDate = request.OpenDate,
            TargetHireDate = request.TargetHireDate,
            Description = request.Description,
            CreatedBy = request.CreatedBy,
            CreatedAt = now,
            UpdatedBy = request.CreatedBy,
            UpdatedAt = now
        };

        var createResult = await requisitionRepository.CreateOneAsync(requisition, cancellationToken);
        if (createResult.TryPickT1(out var exception, out _))
            return HrErrorResponses.FromSaveResult(exception, HrBusinessErrorCodes.RequisitionConcurrencyConflict);

        var saveResult = await unitOfWork.SaveChangesAsync(cancellationToken);
        if (saveResult.TryPickT1(out var saveException, out _))
            return HrErrorResponses.FromSaveResult(saveException, HrBusinessErrorCodes.RequisitionConcurrencyConflict);

        return mapper.ToResponse(requisition);
    }
}
