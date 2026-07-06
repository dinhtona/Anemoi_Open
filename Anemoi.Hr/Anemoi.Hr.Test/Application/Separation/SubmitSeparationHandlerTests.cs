using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.BuildingBlock.Application.Results;
using Anemoi.Contract.Identity.ModelIds;
using Anemoi.Hr.Application.Abstractions;
using Anemoi.Hr.Application.Cqrs.Commands.SeparationCommands.SubmitSeparation;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Domain.Employees;
using Anemoi.Hr.Domain.Separations;
using Anemoi.Hr.Domain.Workflow;
using Anemoi.Hr.ModelIds.ModelIds;
using FluentAssertions;
using NSubstitute;
using OneOf;
using Xunit;

namespace Anemoi.Hr.Test.Application.Separation;

public class SubmitSeparationHandlerTests
{
    private readonly ISqlRepository<Employee> _employeeRepository;
    private readonly ISqlRepository<EmployeeSeparation> _separationRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IWorkflowEngine _workflowEngine;
    private readonly ICurrentUser _currentUser;
    private readonly EmployeeSeparationMapper _mapper;
    private readonly SubmitSeparationHandler _handler;

    public SubmitSeparationHandlerTests()
    {
        _employeeRepository = Substitute.For<ISqlRepository<Employee>>();
        _separationRepository = Substitute.For<ISqlRepository<EmployeeSeparation>>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _workflowEngine = Substitute.For<IWorkflowEngine>();
        _currentUser = Substitute.For<ICurrentUser>();
        _mapper = new EmployeeSeparationMapper();
        _handler = new SubmitSeparationHandler(
            _employeeRepository, _separationRepository, _unitOfWork,
            _workflowEngine, _currentUser, _mapper);
    }

    private static Employee CreateEmployee()
    {
        return new Employee
        {
            Id = new EmployeeId(Guid.NewGuid()),
            DisplayName = "Test Employee",
            EmployeeCode = "EMP-001"
        };
    }

    [Fact]
    public async Task Handle_WorkflowStartFails_ShouldReturnErrorAndNotSave()
    {
        var employee = CreateEmployee();
        var userId = Guid.NewGuid().ToString();

        _currentUser.UserId.Returns(userId);

        _employeeRepository.GetFirstByConditionAsync(
                Arg.Any<System.Linq.Expressions.Expression<Func<Employee, bool>>>(),
                null, Arg.Any<CancellationToken>())
            .Returns(employee);

        _separationRepository.CreateOneAsync(
                Arg.Any<EmployeeSeparation>(),
                Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                var sep = callInfo.Arg<EmployeeSeparation>();
                return Task.FromResult<OneOf<EmployeeSeparation, Exception>>(sep);
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

        var command = new SubmitSeparationCommand(
            employee.Id, "Voluntary", "Personal reasons",
            new DateOnly(2026, 7, 15));

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsT1.Should().BeTrue();
        result.AsT1.Code.Should().Be("HR_WORKFLOW_APPROVER_NOT_FOUND");

        await _unitOfWork.DidNotReceive()
            .SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
