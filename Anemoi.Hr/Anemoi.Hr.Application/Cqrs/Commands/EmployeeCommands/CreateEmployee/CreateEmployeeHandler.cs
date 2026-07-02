using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.BuildingBlock.Application.Helpers;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Departments;
using Anemoi.Hr.Domain.Employees;
using Anemoi.Hr.Domain.Positions;
using Anemoi.Hr.ModelIds.ModelIds;
using OneOf;

namespace Anemoi.Hr.Application.Cqrs.Commands.EmployeeCommands.CreateEmployee;

public sealed class CreateEmployeeHandler(
    ISqlRepository<Employee> employeeRepository,
    ISqlRepository<EmployeeHistory> employeeHistoryRepository,
    ISqlRepository<Department> departmentRepository,
    ISqlRepository<Position> positionRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<CreateEmployeeCommand, OneOf<CreateEmployeeResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<CreateEmployeeResponse, ErrorDetailResponse>> Handle(
        CreateEmployeeCommand request, CancellationToken ct)
    {
        var existingCode = await employeeRepository.GetFirstByConditionAsync(
            x => x.EmployeeCode == request.EmployeeCode, null, ct);
        if (existingCode is not null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.EmployeeCodeAlreadyExists);

        var existingEmail = await employeeRepository.GetFirstByConditionAsync(
            x => x.WorkEmail == request.WorkEmail, null, ct);
        if (existingEmail is not null)
            return HrErrorResponses.Create(HrBusinessErrorCodes.WorkEmailAlreadyExists);

        var department = await departmentRepository.GetFirstByConditionAsync(
            x => x.Id == request.PrimaryDepartmentId, null, ct);
        if (department is null || !department.IsActive)
            return HrErrorResponses.Create(HrBusinessErrorCodes.DepartmentNotFound);

        var position = await positionRepository.GetFirstByConditionAsync(
            x => x.Id == request.PrimaryPositionId, null, ct);
        if (position is null || !position.IsActive)
            return HrErrorResponses.Create(HrBusinessErrorCodes.PositionNotFound);

        if (request.DirectManagerEmployeeId is not null)
        {
            var manager = await employeeRepository.GetFirstByConditionAsync(
                x => x.Id == request.DirectManagerEmployeeId, null, ct);
            if (manager is null)
                return HrErrorResponses.Create(HrBusinessErrorCodes.ManagerNotFound);
        }

        var employeeId = new EmployeeId(IdGenerator.NextGuid());
        var now = DateTime.UtcNow;

        var employee = new Employee
        {
            Id = employeeId,
            EmployeeCode = request.EmployeeCode,
            FirstName = request.FirstName,
            LastName = request.LastName,
            DisplayName = request.DisplayName,
            AvatarStorageKey = request.AvatarStorageKey,
            WorkEmail = request.WorkEmail,
            PersonalEmail = request.PersonalEmail,
            PhoneNumber = request.PhoneNumber,
            DateOfBirth = request.DateOfBirth,
            JoinDate = request.JoinDate,
            EmploymentStatusCode = EmploymentStatusCode.Draft,
            EmploymentTypeCode = request.EmploymentTypeCode,
            GradeCode = request.GradeCode,
            PrimaryDepartmentId = request.PrimaryDepartmentId,
            PrimaryPositionId = request.PrimaryPositionId,
            DirectManagerEmployeeId = request.DirectManagerEmployeeId,
            CreatedAt = now,
            UpdatedAt = now
        };

        await employeeRepository.CreateOneAsync(employee, ct);

        var history = new EmployeeHistory
        {
            Id = new EmployeeHistoryId(IdGenerator.NextGuid()),
            EmployeeId = employeeId,
            EntityType = "Employee",
            EventType = "Created",
            Title = "Employee created",
            Description = $"{request.FirstName} {request.LastName} ({request.EmployeeCode})",
            OccurredAt = now,
            ActorUserId = Guid.TryParse(request.CreatedBy, out var actorGuid) ? actorGuid : null,
        };
        await employeeHistoryRepository.CreateOneAsync(history, ct);

        var saveResult = await unitOfWork.SaveChangesAsync(ct);
        if (saveResult.IsT1)
            return HrErrorResponses.FromSaveResult(saveResult.AsT1, HrBusinessErrorCodes.SaveChangesFailed);

        return new CreateEmployeeResponse(employeeId);
    }
}
