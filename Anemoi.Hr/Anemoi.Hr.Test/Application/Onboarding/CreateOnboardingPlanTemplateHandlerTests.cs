using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Results;
using Anemoi.Hr.Application.Cqrs.Commands.OnboardingCommands.CreateOnboardingPlanTemplate;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Domain.Onboarding;
using FluentAssertions;
using NSubstitute;
using OneOf;
using Xunit;

namespace Anemoi.Hr.Test.Application.Onboarding;

public class CreateOnboardingPlanTemplateHandlerTests
{
    private readonly ISqlRepository<OnboardingPlanTemplate> _templateRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly OnboardingMapper _mapper;
    private readonly CreateOnboardingPlanTemplateHandler _handler;

    public CreateOnboardingPlanTemplateHandlerTests()
    {
        _templateRepository = Substitute.For<ISqlRepository<OnboardingPlanTemplate>>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _mapper = new OnboardingMapper();
        _handler = new CreateOnboardingPlanTemplateHandler(_templateRepository, _unitOfWork, _mapper);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldCreateTemplate()
    {
        var command = new CreateOnboardingPlanTemplateCommand(
            "Standard Onboarding",
            "Standard plan",
            new List<CreateOnboardingTaskTemplateDto>
            {
                new("Setup workstation", null, AssigneeRoleCode.It, 0, 1, true)
            })
        {
            CreatedBy = "user1"
        };

        _templateRepository.CreateOneAsync(
                Arg.Any<OnboardingPlanTemplate>(),
                Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                var template = callInfo.Arg<OnboardingPlanTemplate>();
                return Task.FromResult<OneOf<OnboardingPlanTemplate, Exception>>(template);
            });

        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<OneOf<None, Exception>>(new None()));

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsT0.Should().BeTrue();
        var response = result.AsT0;
        response.Name.Should().Be("Standard Onboarding");
        response.Description.Should().Be("Standard plan");
        response.Status.Should().Be(OnboardingPlanTemplateStatusCode.Active);
        response.Version.Should().Be(1);
        response.TaskTemplates.Should().ContainSingle();
    }

    [Fact]
    public async Task Handle_WithNoTaskTemplates_ShouldCreateEmptyTemplate()
    {
        var command = new CreateOnboardingPlanTemplateCommand(
            "Minimal Plan",
            null,
            new List<CreateOnboardingTaskTemplateDto>())
        {
            CreatedBy = "user1"
        };

        _templateRepository.CreateOneAsync(
                Arg.Any<OnboardingPlanTemplate>(),
                Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                var template = callInfo.Arg<OnboardingPlanTemplate>();
                return Task.FromResult<OneOf<OnboardingPlanTemplate, Exception>>(template);
            });

        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<OneOf<None, Exception>>(new None()));

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsT0.Should().BeTrue();
        var response = result.AsT0;
        response.Name.Should().Be("Minimal Plan");
        response.TaskTemplates.Should().BeEmpty();
    }
}
