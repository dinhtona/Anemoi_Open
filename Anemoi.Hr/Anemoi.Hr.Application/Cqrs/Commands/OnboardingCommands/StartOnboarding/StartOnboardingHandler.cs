using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.Cqrs.Commands;
using Anemoi.BuildingBlock.Application.Helpers;
using Anemoi.BuildingBlock.Application.Responses;
using Anemoi.Hr.Application.Configurations;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Responses;
using Anemoi.Hr.Domain.Onboarding;
using Anemoi.Hr.Domain.Employees;
using Anemoi.Hr.ModelIds.ModelIds;
using MediatR;
using OneOf;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Anemoi.Hr.Application.Cqrs.Commands.OnboardingCommands.StartOnboarding;

public sealed class StartOnboardingHandler(
    ISqlRepository<OnboardingPlanTemplate> templateRepository,
    ISqlRepository<OnboardingInstance> instanceRepository,
    ISqlRepository<Employee> employeeRepository,
    IUnitOfWork unitOfWork,
    OnboardingMapper mapper)
    : ICommandHandler<StartOnboardingCommand, OneOf<OnboardingInstanceResponse, ErrorDetailResponse>>
{
    public async Task<OneOf<OnboardingInstanceResponse, ErrorDetailResponse>> Handle(
        StartOnboardingCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var template = await templateRepository.GetFirstByConditionAsync(
                x => x.Id == request.TemplateId, null, cancellationToken);
            if (template == null)
                return HrErrorResponses.Create(HrBusinessErrorCodes.HrOnboardingTemplateNotFound);
            if (!template.IsActive())
                return HrErrorResponses.Create(HrBusinessErrorCodes.HrOnboardingTemplateInactive);
            if (template.TaskTemplates.Count == 0)
                return HrErrorResponses.Create(HrBusinessErrorCodes.HrOnboardingTemplateHasNoTasks);

            var employee = await employeeRepository.GetFirstByConditionAsync(
                x => x.Id == request.EmployeeId, null, cancellationToken);
            if (employee == null)
                return HrErrorResponses.Create(HrBusinessErrorCodes.HrOnboardingEmployeeNotFound);

            var existingOnboarding = await instanceRepository.GetFirstByConditionAsync(
                x => x.EmployeeId == request.EmployeeId &&
                     (x.Status == OnboardingInstanceStatusCode.Draft ||
                      x.Status == OnboardingInstanceStatusCode.InProgress), null, cancellationToken);
            if (existingOnboarding != null)
                return HrErrorResponses.Create(HrBusinessErrorCodes.HrOnboardingEmployeeAlreadyOnboarding);

            var instanceId = new OnboardingInstanceId(IdGenerator.NextGuid());
            var instance = OnboardingInstance.Create(
                instanceId, request.EmployeeId, request.TemplateId,
                template.Name, template.Version, request.StartDate, request.CreatedBy);

            foreach (var taskTemplate in template.TaskTemplates.OrderBy(t => t.SortOrder))
            {
                var taskId = new OnboardingTaskId(IdGenerator.NextGuid());
                var dueDate = request.StartDate.AddDays(taskTemplate.OffsetDays);

                string? assignedUserId = null;
                string? assignedUserDisplayName = null;
                if (taskTemplate.AssigneeRoleCode != null &&
                    request.RoleMappings.TryGetValue(taskTemplate.AssigneeRoleCode, out var resolvedUserId))
                {
                    assignedUserId = resolvedUserId;
                    assignedUserDisplayName = resolvedUserId;
                }
                else if (taskTemplate.AssigneeRoleCode != null)
                {
                    return HrErrorResponses.Create(HrBusinessErrorCodes.HrOnboardingResolveRoleMissing);
                }

                var task = OnboardingTask.Create(
                    taskId, taskTemplate.Title, taskTemplate.Description,
                    taskTemplate.AssigneeRoleCode, assignedUserId, assignedUserDisplayName,
                    request.CreatedBy, dueDate, taskTemplate.SortOrder, taskTemplate.IsRequired);

                instance.AddTask(task);
            }

            await instanceRepository.CreateOneAsync(instance, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return mapper.ToResponse(instance);
        }
        catch (Exception exception)
        {
            return HrErrorResponses.FromSaveResult(exception, HrBusinessErrorCodes.SaveChangesFailed);
        }
    }
}
