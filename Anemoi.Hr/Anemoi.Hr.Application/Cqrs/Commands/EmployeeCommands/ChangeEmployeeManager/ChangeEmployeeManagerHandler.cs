using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.BuildingBlock.Application.Helpers;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.BuildingBlock.Application.Results;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Domain.Employees;
using Anemoi.Hr.ModelIds.ModelIds;
using OneOf;

namespace Anemoi.Hr.Application.Cqrs.Commands.EmployeeCommands.ChangeEmployeeManager;

public sealed class ChangeEmployeeManagerHandler(
    ISqlRepository<Employee> employeeRepository,
    ISqlRepository<EmployeeHistory> employeeHistoryRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<ChangeEmployeeManagerCommand, OneOf<None, ErrorDetailResponse>>
{
    public async Task<OneOf<None, ErrorDetailResponse>> Handle(
        ChangeEmployeeManagerCommand request, CancellationToken ct)
    {
        var employee = await employeeRepository.GetFirstByConditionAsync(
            x => x.Id == request.EmployeeId, null, ct);
        if (employee is null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.EmployeeNotFound);

        if (request.NewManagerEmployeeId is not null)
        {
            var manager = await employeeRepository.GetFirstByConditionAsync(
                x => x.Id == request.NewManagerEmployeeId, null, ct);
            if (manager is null)
                return HrErrorResponses.Create(HrBusinessErrorCodes.ManagerNotFound);
        }

        employee.DirectManagerEmployeeId = request.NewManagerEmployeeId;
        employee.UpdatedAt = DateTime.UtcNow;

        var history = new EmployeeHistory
        {
            Id = new EmployeeHistoryId(IdGenerator.NextGuid()),
            EmployeeId = request.EmployeeId,
            EntityType = "Employee",
            EntityId = request.EmployeeId.Value.ToString(),
            EventType = "ManagerChanged",
            Title = "Employee manager changed",
            Description = "Manager changed",
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
