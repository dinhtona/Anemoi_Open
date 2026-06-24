using Anemoi.Hr.Application.Configurations;
using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Infrastructure.GeneralInstaller;
using Anemoi.Hr.Application.Cqrs.Commands.LeaveRequestCommands.ApproveLeaveRequest;
using Anemoi.Hr.Application.Cqrs.Commands.LeaveRequestCommands.CancelLeaveRequest;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Abstractions;
using Anemoi.Hr.Application.Cqrs.Queries.PayrollReportingQueries.Shared;
using Anemoi.Hr.Application.Services;
using Anemoi.Hr.Application.WorkflowTargetStatusUpdaters;
using Anemoi.Hr.Application.Cqrs.Events;
using Anemoi.Hr.Domain.Workflow;
using Anemoi.Hr.Infrastructure.Reporting;
using Anemoi.Hr.Infrastructure.Services;
using Anemoi.Hr.Domain;
using Anemoi.Hr.Infrastructure.Persistence;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Anemoi.Hr.Infrastructure.Installers;

public sealed class ServiceInstaller : IInstaller
{
    public void InstallerServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<INotificationActionHandler, LeaveNotificationActionHandler>();
        services.AddScoped<INotificationActionHandler, OvertimeNotificationActionHandler>();
        services.AddScoped<INotificationActionHandler, PayrollNotificationActionHandler>();
        services.AddScoped<INotificationActionHandler, OnboardingNotificationActionHandler>();
        services.AddScoped<INotificationActionHandlerRegistry, NotificationActionHandlerRegistry>();
        services.AddScoped<LeaveMapper>();
        services.AddScoped<EmployeeMapper>();
        services.AddScoped<EmployeeContractMapper>();
        services.AddScoped<CompensationMapper>();
        services.AddScoped<PayrollMapper>();
        services.AddScoped<AttendanceMapper>();
        services.AddScoped<PayslipMapper>();
        services.AddScoped<PayslipDocumentMapper>();
        services.AddScoped<PayrollReportingMapper>();
        services.AddScoped<TaxationMapper>();
        services.AddScoped<InsuranceMapper>();
        services.AddScoped<EssMapper>();
        services.AddScoped<WorkflowMapper>();
        services.AddScoped<MasterDataMapper>();
        services.AddScoped<WorkflowRoleAssignmentMapper>();
        services.AddScoped<ProbationRecordMapper>();
        services.AddScoped<EmployeeTransferMapper>();
        services.AddScoped<EmployeeSeparationMapper>();
        services.AddScoped<EmployeeHistoryMapper>();
        services.AddScoped<PayrollReportExportService>();
        services.AddScoped<IReportExporter, CsvReportExporter>();
        services.AddScoped<IPayslipDocumentStorage, LocalPayslipDocumentStorage>();
        services.AddScoped<IPayslipPdfRenderer, PayslipPdfRenderer>();
        var hrSettings = configuration.GetSection(nameof(HrSettings)).Get<HrSettings>() ?? new HrSettings();
        if (hrSettings.UseSmtp)
        {
            services.AddScoped<IPayslipEmailSender, SmtpPayslipEmailSender>();
        }
        else
        {
            services.AddScoped<IPayslipEmailSender, LoggingPayslipEmailSender>();
        }
        services.AddScoped<ICurrentUser, CurrentUser>();
        services.AddHttpContextAccessor();
        services.AddScoped<ApproveLeaveRequestHandler>();
        services.AddScoped<CancelLeaveRequestHandler>();
        services.AddScoped<Anemoi.Hr.Application.Abstractions.IEmployeeGradeLookup, Anemoi.Hr.Application.Services.EmployeeGradeLookup>();
        services.AddScoped<IWorkflowEngine, WorkflowEngine>();
        services.AddScoped<IWorkflowHierarchyResolver, WorkflowHierarchyResolver>();
        services.AddScoped<IWorkflowBuilder, WorkflowBuilder>();
        services.AddScoped<IApprovalResolver, DefaultApprovalResolver>();
        services.AddScoped<IWorkflowRoleResolver, DefaultWorkflowRoleResolver>();
        services.AddScoped<IWorkflowQueryService, WorkflowQueryService>();
        services.AddScoped<IOrganizationService, OrganizationService>();
        services.AddScoped<IWorkflowTargetStatusUpdater, LeaveWorkflowStatusUpdater>();
        services.AddScoped<IWorkflowTargetStatusUpdater, OvertimeWorkflowStatusUpdater>();
        services.AddScoped<IWorkflowTargetStatusUpdater, PayrollWorkflowStatusUpdater>();
        services.AddScoped<IWorkflowTargetStatusUpdater, RecruitmentWorkflowStatusUpdater>();
        services.AddScoped<IWorkflowTargetStatusUpdater, EmployeeTransferWorkflowStatusUpdater>();
        services.AddScoped<IWorkflowTargetStatusUpdater, EmployeeSeparationWorkflowStatusUpdater>();
        services.AddScoped<INotificationHandler<WorkflowInstanceApprovedDomainEvent>, WorkflowInstanceApprovedHandler>();
        services.AddScoped<INotificationHandler<WorkflowInstanceRejectedDomainEvent>, WorkflowInstanceRejectedHandler>();
        services.AddScoped<INotificationHandler<WorkflowInstanceApprovedDomainEvent>, WorkflowApprovedIntegrationEventPublisher>();
        services.AddScoped<INotificationHandler<WorkflowInstanceRejectedDomainEvent>, WorkflowRejectedIntegrationEventPublisher>();
        services.AddEfRepositoriesAsScope<HrDbContext>(typeof(IHrDomainAssemblyMarker).Assembly);
        services.AddEfUnitOfWorkAsScope<HrDbContext>();
    }
}
