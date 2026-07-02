using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.BuildingBlock.Application.Results;
using Anemoi.Contract.Identity.ModelIds;
using Anemoi.Hr.Application.Abstractions;
using Anemoi.Hr.Application.Cqrs.Commands.TransferCommands.SubmitTransfer;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Domain.Departments;
using Anemoi.Hr.Domain.Employees;
using Anemoi.Hr.Domain.Positions;
using Anemoi.Hr.Domain.Transfers;
using Anemoi.Hr.Domain.Workflow;
using Anemoi.Hr.ModelIds.ModelIds;
using FluentAssertions;
using NSubstitute;
using OneOf;
using Xunit;

namespace Anemoi.Hr.Test.Application.Transfer;

public class SubmitTransferHandlerTests
{
    private readonly ISqlRepository<Employee> _employeeRepository;
    private readonly ISqlRepository<Department> _departmentRepository;
    private readonly ISqlRepository<Position> _positionRepository;
    private readonly ISqlRepository<EmployeeTransfer> _transferRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IWorkflowEngine _workflowEngine;
    private readonly ICurrentUser _currentUser;
    private readonly EmployeeTransferMapper _mapper;
    private readonly SubmitTransferHandler _handler;

    public SubmitTransferHandlerTests()
    {
        _employeeRepository = Substitute.For<ISqlRepository<Employee>>();
        _departmentRepository = Substitute.For<ISqlRepository<Department>>();
        _positionRepository = Substitute.For<ISqlRepository<Position>>();
        _transferRepository = Substitute.For<ISqlRepository<EmployeeTransfer>>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _workflowEngine = Substitute.For<IWorkflowEngine>();
        _currentUser = Substitute.For<ICurrentUser>();
        _mapper = new EmployeeTransferMapper();
        _handler = new SubmitTransferHandler(
            _employeeRepository, _departmentRepository, _positionRepository,
            _transferRepository, _unitOfWork, _workflowEngine,
            _currentUser, _mapper);
    }

    private static Employee CreateEmployee()
    {
        return new Employee
        {
            Id = new EmployeeId(Guid.NewGuid()),
            DisplayName = "Test Employee",
            EmployeeCode = "EMP-001",
            PrimaryDepartmentId = new DepartmentId(Guid.NewGuid()),
            PrimaryPositionId = new PositionId(Guid.NewGuid()),
            DirectManagerEmployeeId = new EmployeeId(Guid.NewGuid()),
            GradeCode = "G1"
        };
    }

    private static Department CreateDepartment()
    {
        return Department.Create(
            new DepartmentId(Guid.NewGuid()),
            "DEPT-001", "Test Department", "Division",
            null, null);
    }

    private static Position CreatePosition()
    {
        return Position.Create(
            new PositionId(Guid.NewGuid()),
            new DepartmentId(Guid.NewGuid()),
            "POS-001", "Test Position", "FullTime");
    }

    [Fact]
    public async Task Handle_WorkflowStartFails_ShouldReturnErrorAndNotSave()
    {
        var employee = CreateEmployee();
        var department = CreateDepartment();
        var position = CreatePosition();
        var userId = Guid.NewGuid().ToString();

        _currentUser.UserId.Returns(userId);

        _employeeRepository.GetFirstByConditionAsync(
                Arg.Any<System.Linq.Expressions.Expression<Func<Employee, bool>>>(),
                null, Arg.Any<CancellationToken>())
            .Returns(employee);

        _departmentRepository.GetFirstByConditionAsync(
                Arg.Any<System.Linq.Expressions.Expression<Func<Department, bool>>>(),
                null, Arg.Any<CancellationToken>())
            .Returns(department);

        _positionRepository.GetFirstByConditionAsync(
                Arg.Any<System.Linq.Expressions.Expression<Func<Position, bool>>>(),
                null, Arg.Any<CancellationToken>())
            .Returns(position);

        _transferRepository.CreateOneAsync(
                Arg.Any<EmployeeTransfer>(),
                Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                var transfer = callInfo.Arg<EmployeeTransfer>();
                return Task.FromResult<OneOf<EmployeeTransfer, Exception>>(transfer);
            });

        var workflowError = new ErrorDetailResponse
        {
            Code = "HR_WORKFLOW_APPROVER_NOT_FOUND"
        };

        _workflowEngine.StartAsync(
                Arg.Any<string>(),
                Arg.Any<Guid>(),
                Arg.Any<EmployeeId>(),
                Arg.Any<UserId>(),
                Arg.Any<UserId>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<OneOf<WorkflowInstance, ErrorDetailResponse>>(workflowError));

        var command = new SubmitTransferCommand(
            employee.Id,
            department.Id,
            position.Id,
            new EmployeeId(Guid.NewGuid()),
            "G2",
            new DateOnly(2026, 8, 1),
            "Career growth");

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsT1.Should().BeTrue();
        result.AsT1.Code.Should().Be("HR_WORKFLOW_APPROVER_NOT_FOUND");

        await _unitOfWork.DidNotReceive()
            .SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
