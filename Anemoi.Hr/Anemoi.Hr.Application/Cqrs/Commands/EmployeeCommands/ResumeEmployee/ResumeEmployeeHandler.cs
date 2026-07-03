using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.BuildingBlock.Application.Helpers;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.BuildingBlock.Application.Results;
using Anemoi.BuildingBlock.Domain;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Domain.Employees;
using Anemoi.Hr.ModelIds.ModelIds;
using OneOf;

namespace Anemoi.Hr.Application.Cqrs.Commands.EmployeeCommands.ResumeEmployee;

public sealed class ResumeEmployeeHandler(
    ISqlRepository<Employee> employeeRepository,
    ISqlRepository<EmployeeHistory> employeeHistoryRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<ResumeEmployeeCommand, OneOf<None, ErrorDetailResponse>>
{
    public async Task<OneOf<None, ErrorDetailResponse>> Handle(
        ResumeEmployeeCommand request, CancellationToken ct)
    {
        var employee = await employeeRepository.GetFirstByConditionAsync(
            x => x.Id == request.EmployeeId, null, ct);
        if (employee is null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.EmployeeNotFound);

        try
        {
            employee.Resume(request.CreatedBy ?? PayrollConstants.SystemActor);
        }
        catch (DomainException)
        {
            return HrErrorResponses.Create(HrBusinessErrorCodes.InvalidStatusTransition);
        }

        var history = new EmployeeHistory
        {
            Id = new EmployeeHistoryId(IdGenerator.NextGuid()),
            EmployeeId = request.EmployeeId,
            EntityType = "Employee",
            EntityId = request.EmployeeId.Value.ToString(),
            EventType = "Resumed",
            Title = "Employee resumed",
            Description = "Employee resumed",
            OccurredAt = DateTime.UtcNow,
            ActorUserId = Guid.TryParse(request.CreatedBy, out var actorGuid) ? actorGuid : null,
        };
        await employeeHistoryRepository.CreateOneAsync(history, ct);

        var saveResult = await unitOfWork.SaveChangesAsync(ct);
        if (saveResult.IsT1)
            return HrErrorResponses.FromSaveResult(saveResult.AsT1, HrBusinessErrorCodes.SaveChangesFailed);

        return None.Value;
    }
}
