using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Infrastructure.GeneralInstaller;
using Anemoi.Hr.Application.Cqrs.Commands.LeaveRequestCommands.ApproveLeaveRequest;
using Anemoi.Hr.Application.Cqrs.Commands.LeaveRequestCommands.CancelLeaveRequest;
using Anemoi.Hr.Application.Mappings;
using Anemoi.Hr.Application.Abstractions;
using Anemoi.Hr.Application.Cqrs.Queries.PayrollReportingQueries.Shared;
using Anemoi.Hr.Infrastructure.Reporting;
using Anemoi.Hr.Infrastructure.Services;
using Anemoi.Hr.Domain;
using Anemoi.Hr.Infrastructure.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Anemoi.Hr.Infrastructure.Installers;

public sealed class ServiceInstaller : IInstaller
{
    public void InstallerServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<LeaveMapper>();
        services.AddScoped<EmployeeMapper>();
        services.AddScoped<EmployeeContractMapper>();
        services.AddScoped<CompensationMapper>();
        services.AddScoped<PayrollMapper>();
        services.AddScoped<AttendanceMapper>();
        services.AddScoped<PayslipMapper>();
        services.AddScoped<PayrollReportingMapper>();
        services.AddScoped<TaxationMapper>();
        services.AddScoped<PayrollReportExportService>();
        services.AddScoped<IReportExporter, CsvReportExporter>();
        services.AddScoped<ICurrentUser, CurrentUser>();
        services.AddHttpContextAccessor();
        services.AddScoped<ApproveLeaveRequestHandler>();
        services.AddScoped<CancelLeaveRequestHandler>();
        services.AddScoped<Anemoi.Hr.Application.Abstractions.IEmployeeGradeLookup, Anemoi.Hr.Application.Services.EmployeeGradeLookup>();
        services.AddEfRepositoriesAsScope<HrDbContext>(typeof(IHrDomainAssemblyMarker).Assembly);
        services.AddEfUnitOfWorkAsScope<HrDbContext>();
    }
}
