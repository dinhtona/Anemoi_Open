using Anemoi.BuildingBlock.Application.Abstractions;
using Anemoi.BuildingBlock.Application.BulkImport.Abstractions;
using Anemoi.BuildingBlock.Application.BulkImport.Services;
using Anemoi.Hr.Application.BulkImport.Abstractions;
using Anemoi.Hr.Application.BulkImport.EmployeeImport;
using Anemoi.Hr.Application.BulkImport.EmployeeImport.Validation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Anemoi.Hr.Infrastructure.Installers;

public sealed class EmployeeImportServiceInstaller : IInstaller
{
    public void InstallerServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<BulkExcelParserService>();
        services.AddScoped<BulkExcelTemplateService>();

        services.AddScoped<SyntaxValidator>();
        services.AddScoped<BusinessValidator>();
        services.AddScoped<DuplicateValidator>();
        services.AddScoped<ReferenceValidator>();

        services.AddScoped<EmployeeImportMappingService>();
        services.AddScoped<EmployeeImportHandler>();
        services.AddScoped<EmployeeImportTemplateProvider>();
        services.AddScoped<EmployeeImportErrorFileGenerator>();

        services.AddScoped<IImportOrchestrator, EmployeeImportOrchestrator>();
    }
}
