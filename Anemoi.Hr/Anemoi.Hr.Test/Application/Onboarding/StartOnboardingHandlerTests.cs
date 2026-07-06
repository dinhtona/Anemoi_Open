using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Results;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Cqrs.Commands.OnboardingCommands.StartOnboarding;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Domain.Employees;
using Anemoi.Hr.Domain.Onboarding;
using Anemoi.Hr.ModelIds.ModelIds;
using FluentAssertions;
using NSubstitute;
using OneOf;
using Xunit;

namespace Anemoi.Hr.Test.Application.Onboarding;

public class StartOnboardingHandlerTests
{
    private readonly ISqlRepository<OnboardingPlanTemplate> _templateRepository;
    private readonly ISqlRepository<OnboardingInstance> _instanceRepository;
    private readonly ISqlRepository<Employee> _employeeRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly OnboardingMapper _mapper;
    private readonly StartOnboardingHandler _handler;

    public StartOnboardingHandlerTests()
    {
        _templateRepository = Substitute.For<ISqlRepository<OnboardingPlanTemplate>>();
        _instanceRepository = Substitute.For<ISqlRepository<OnboardingInstance>>();
        _employeeRepository = Substitute.For<ISqlRepository<Employee>>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _mapper = new OnboardingMapper();
        _handler = new StartOnboardingHandler(
            _templateRepository, _instanceRepository, _employeeRepository,
            _unitOfWork, _mapper);
    }

    private OnboardingPlanTemplate CreateActiveTemplateWithTasks()
    {
        var templateId = new OnboardingPlanTemplateId(Guid.NewGuid());
        var template = OnboardingPlanTemplate.Create(
            templateId, "Standard Onboarding", null, "user1");
        var task = OnboardingTaskTemplate.Create(
            new OnboardingTaskTemplateId(Guid.NewGuid()),
            "Setup workstation", null, AssigneeRoleCode.It, 0, 1);
        template.AddTaskTemplate(task);
        return template;
    }

    private Employee CreateEmployee()
    {
        return new Employee
        {
            Id = new EmployeeId(Guid.NewGuid()),
            DisplayName = "John Doe",
            EmployeeCode = "EMP-001"
        };
    }

    [Fact]
    public async Task Handle_WithValidRequest_ShouldStartOnboarding()
    {
        var template = CreateActiveTemplateWithTasks();
        var employee = CreateEmployee();
        var templateId = template.Id;
        var employeeId = employee.Id;
        var roleMappings = new Dictionary<string, string>
        {
            { AssigneeRoleCode.It, "user2" }
        };

        _templateRepository.GetFirstByConditionAsync(
                Arg.Any<System.Linq.Expressions.Expression<Func<OnboardingPlanTemplate, bool>>>(),
                null, Arg.Any<CancellationToken>())
            .Returns(template);

        _employeeRepository.GetFirstByConditionAsync(
                Arg.Any<System.Linq.Expressions.Expression<Func<Employee, bool>>>(),
                null, Arg.Any<CancellationToken>())
            .Returns(employee);

        _instanceRepository.GetFirstByConditionAsync(
                Arg.Any<System.Linq.Expressions.Expression<Func<OnboardingInstance, bool>>>(),
                null, Arg.Any<CancellationToken>())
            .Returns((OnboardingInstance)null);

        _instanceRepository.CreateOneAsync(
                Arg.Any<OnboardingInstance>(),
                Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                var instance = callInfo.Arg<OnboardingInstance>();
                return Task.FromResult<OneOf<OnboardingInstance, Exception>>(instance);
            });

        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<OneOf<None, Exception>>(new None()));

        var command = new StartOnboardingCommand(
            employeeId, templateId, DateTime.UtcNow, roleMappings)
        {
            CreatedBy = "user1"
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsT0.Should().BeTrue();
        var response = result.AsT0;
        response.Status.Should().Be(OnboardingInstanceStatusCode.InProgress);
        response.TemplateName.Should().Be("Standard Onboarding");
    }

    [Fact]
    public async Task Handle_TemplateInactive_ShouldReturnError()
    {
        var templateId = new OnboardingPlanTemplateId(Guid.NewGuid());
        var template = OnboardingPlanTemplate.Create(
            templateId, "Inactive Template", null, "user1");
        template.Deactivate();

        var employeeId = new EmployeeId(Guid.NewGuid());

        _templateRepository.GetFirstByConditionAsync(
                Arg.Any<System.Linq.Expressions.Expression<Func<OnboardingPlanTemplate, bool>>>(),
                null, Arg.Any<CancellationToken>())
            .Returns(template);

        var command = new StartOnboardingCommand(
            employeeId, templateId, DateTime.UtcNow,
            new Dictionary<string, string>())
        {
            CreatedBy = "user1"
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsT1.Should().BeTrue();
        result.AsT1.Code.Should().Be(HrBusinessErrorCodes.HrOnboardingTemplateInactive);
    }

    [Fact]
    public async Task Handle_EmployeeAlreadyOnboarding_ShouldReturnError()
    {
        var template = CreateActiveTemplateWithTasks();
        var employee = CreateEmployee();
        var employeeId = employee.Id;

        _templateRepository.GetFirstByConditionAsync(
                Arg.Any<System.Linq.Expressions.Expression<Func<OnboardingPlanTemplate, bool>>>(),
                null, Arg.Any<CancellationToken>())
            .Returns(template);

        _employeeRepository.GetFirstByConditionAsync(
                Arg.Any<System.Linq.Expressions.Expression<Func<Employee, bool>>>(),
                null, Arg.Any<CancellationToken>())
            .Returns(employee);

        _instanceRepository.GetFirstByConditionAsync(
                Arg.Any<System.Linq.Expressions.Expression<Func<OnboardingInstance, bool>>>(),
                null, Arg.Any<CancellationToken>())
            .Returns(OnboardingInstance.Create(
                new OnboardingInstanceId(Guid.NewGuid()),
                employeeId, template.Id, "Existing", 1,
                DateTime.UtcNow, "user1"));

        var command = new StartOnboardingCommand(
            employeeId, template.Id, DateTime.UtcNow,
            new Dictionary<string, string>())
        {
            CreatedBy = "user1"
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsT1.Should().BeTrue();
        result.AsT1.Code.Should().Be(HrBusinessErrorCodes.HrOnboardingEmployeeAlreadyOnboarding);
    }

    [Fact]
    public async Task Handle_TemplateNotFound_ShouldReturnError()
    {
        _templateRepository.GetFirstByConditionAsync(
                Arg.Any<System.Linq.Expressions.Expression<Func<OnboardingPlanTemplate, bool>>>(),
                null, Arg.Any<CancellationToken>())
            .Returns((OnboardingPlanTemplate)null);

        var command = new StartOnboardingCommand(
            new EmployeeId(Guid.NewGuid()),
            new OnboardingPlanTemplateId(Guid.NewGuid()),
            DateTime.UtcNow,
            new Dictionary<string, string>())
        {
            CreatedBy = "user1"
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsT1.Should().BeTrue();
        result.AsT1.Code.Should().Be(HrBusinessErrorCodes.HrOnboardingTemplateNotFound);
    }

    [Fact]
    public async Task Handle_TemplateHasNoTasks_ShouldReturnError()
    {
        var templateId = new OnboardingPlanTemplateId(Guid.NewGuid());
        var template = OnboardingPlanTemplate.Create(
            templateId, "Empty Template", null, "user1");
        var employee = CreateEmployee();

        _templateRepository.GetFirstByConditionAsync(
                Arg.Any<System.Linq.Expressions.Expression<Func<OnboardingPlanTemplate, bool>>>(),
                null, Arg.Any<CancellationToken>())
            .Returns(template);

        _employeeRepository.GetFirstByConditionAsync(
                Arg.Any<System.Linq.Expressions.Expression<Func<Employee, bool>>>(),
                null, Arg.Any<CancellationToken>())
            .Returns(employee);

        _instanceRepository.GetFirstByConditionAsync(
                Arg.Any<System.Linq.Expressions.Expression<Func<OnboardingInstance, bool>>>(),
                null, Arg.Any<CancellationToken>())
            .Returns((OnboardingInstance)null);

        var command = new StartOnboardingCommand(
            employee.Id, templateId, DateTime.UtcNow,
            new Dictionary<string, string>())
        {
            CreatedBy = "user1"
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsT1.Should().BeTrue();
        result.AsT1.Code.Should().Be(HrBusinessErrorCodes.HrOnboardingTemplateHasNoTasks);
    }
}
