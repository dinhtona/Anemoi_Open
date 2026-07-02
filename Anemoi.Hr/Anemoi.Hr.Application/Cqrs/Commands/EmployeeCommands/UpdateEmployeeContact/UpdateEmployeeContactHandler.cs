using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.BuildingBlock.Application.Helpers;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.BuildingBlock.Application.Results;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Domain.Employees;
using Anemoi.Hr.ModelIds.ModelIds;
using OneOf;

namespace Anemoi.Hr.Application.Cqrs.Commands.EmployeeCommands.UpdateEmployeeContact;

public sealed class UpdateEmployeeContactHandler(
    ISqlRepository<Employee> employeeRepository,
    ISqlRepository<EmployeeHistory> employeeHistoryRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateEmployeeContactCommand, OneOf<None, ErrorDetailResponse>>
{
    public async Task<OneOf<None, ErrorDetailResponse>> Handle(
        UpdateEmployeeContactCommand request, CancellationToken ct)
    {
        var employee = await employeeRepository.GetFirstByConditionAsync(
            x => x.Id == request.EmployeeId, null, ct);
        if (employee is null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.EmployeeNotFound);

        if (request.WorkEmail is not null)
        {
            if (employee.IdentityUserId is not null)
                return HrErrorResponses.Create(HrBusinessErrorCodes.WorkEmailLinkedToIdentity);
            employee.WorkEmail = request.WorkEmail;
        }

        if (request.PersonalEmail is not null)
            employee.PersonalEmail = request.PersonalEmail;

        if (request.PhoneNumber is not null)
            employee.PhoneNumber = request.PhoneNumber;

        if (request.DateOfBirth is not null)
            employee.DateOfBirth = request.DateOfBirth;

        if (request.AvatarStorageKey is not null)
            employee.AvatarStorageKey = request.AvatarStorageKey;

        employee.UpdatedAt = DateTime.UtcNow;

        var history = new EmployeeHistory
        {
            Id = new EmployeeHistoryId(IdGenerator.NextGuid()),
            EmployeeId = request.EmployeeId,
            EntityType = "Employee",
            EventType = "ContactUpdated",
            Title = "Employee contact updated",
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
