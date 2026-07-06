using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.BuildingBlock.Application.Helpers;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Contract.Identity.ModelIds;
using Anemoi.Hr.Application.Abstractions;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Cqrs.Common.Dtos;
using Anemoi.Hr.Domain.Employees;
using Anemoi.Hr.Domain.Probation;
using Anemoi.Hr.ModelIds.ModelIds;
using OneOf;

namespace Anemoi.Hr.Application.Cqrs.Commands.ProbationCommands.StartProbation;

public sealed class StartProbationHandler(
    ISqlRepository<Employee> employeeRepository,
    ISqlRepository<ProbationRecord> probationRepository,
    IUnitOfWork unitOfWork,
    IWorkflowEngine workflowEngine,
    ICurrentUser currentUser,
    Mappings.ProbationRecordMapper mapper)
    : ICommandHandler<StartProbationCommand, OneOf<ProbationRecordDto, ErrorDetailResponse>>
{
    public async Task<OneOf<ProbationRecordDto, ErrorDetailResponse>> Handle(
        StartProbationCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var employee = await employeeRepository.GetFirstByConditionAsync(
                x => x.Id == request.EmployeeId, null, cancellationToken);
            if (employee == null)
                return HrErrorResponses.Create(HrBusinessErrorCodes.EmployeeNotFound);

            var id = new ProbationRecordId(IdGenerator.NextGuid());
            var record = ProbationRecord.Create(id, request.EmployeeId, request.StartDate, request.EndDate);

            await probationRepository.CreateOneAsync(record, cancellationToken);

            var requesterUserId = new UserId(Guid.Parse(currentUser.UserId));
            var workflowResult = await workflowEngine.StartAsync(
                WorkflowConstants.TargetEntityTypes.ProbationRecord,
                record.Id.Value,
                request.EmployeeId,
                requesterUserId,
                requesterUserId,
                cancellationToken);

            if (workflowResult.TryPickT1(out var workflowError, out _))
                return workflowError;

            await unitOfWork.SaveChangesAsync(cancellationToken);

            return mapper.ToDto(record);
        }
        catch (Exception exception)
        {
            return HrErrorResponses.FromSaveResult(exception, HrBusinessErrorCodes.SaveChangesFailed);
        }
    }
}
