using System;
using System.Reflection;
using Anemoi.BuildingBlock.Infrastructure.GeneralInstaller;
using Anemoi.BuildingBlock.Infrastructure.RunSqlMigration;
using Lambda.Identity.Application.SeedData;
using Anemoi.Identity.Infrastructure;
using Anemoi.Identity.Infrastructure.DataContext;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseDefaultServiceProvider((context, provider) =>
    provider.ValidateScopes = provider.ValidateOnBuild = context.HostingEnvironment.IsDevelopment());

builder.Configuration
    .AddUserSecrets(Assembly.GetExecutingAssembly())
    .AddEnvironmentVariables()
    .AddJsonFile("serilogConfiguration.json")
    .AddJsonFile("defaultApplicationPolicy.json");

builder.Host.UseSerilog((host, configuration) => configuration.Enrich
    .FromLogContext()
    .ReadFrom.Configuration(host.Configuration)
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .MinimumLevel.Override("System", LogEventLevel.Information)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", LogEventLevel.Warning));

builder.Services.InstallServicesInAssembly<IIdentityInfrastructureAssemblyMarker>(builder.Configuration);
builder.Services.AddHealthChecks();

// FUNC-007: health endpoints (same pattern as Centralize API).
builder.Services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy(), tags: ["live"])
    .AddCheck("rabbitmq", () =>
    {
        var host = builder.Configuration.GetValue<string>("MassTransitSetting:Host") ?? "localhost";
        try
        {
            using var tcp = new System.Net.Sockets.TcpClient(host, 5672);
            return tcp.Connected
                ? HealthCheckResult.Healthy()
                : HealthCheckResult.Unhealthy("RabbitMQ port unreachable");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("RabbitMQ unreachable", ex);
        }
    }, tags: ["ready"]);

var app = builder.Build();

await MigrationDatabase.MigrationDatabaseAsync<IdentityDbContext>(app);

var serviceScope = app.Services.CreateScope();

await SeedData.SeedRolesAsync(serviceScope);

await EmployeeRoleSeeder.SeedDefaultRoleGroupsAsync(serviceScope);

await SeedData.SeedApplicationPoliciesAsync(serviceScope);

await SeedData.RegisterAdministratorAsync(serviceScope);

if (app.Environment.IsDevelopment())
{
    await SeedData.RegisterDevTestUsersAsync(serviceScope);
}

await SeedData.NormalizeAuthorizationAssignmentsAsync(serviceScope);

await SeedData.RemoveReservedApplicationPolicyClaimsAsync(serviceScope);

app.UseCors("CorsPolicy");

app.UseHttpsRedirection();

// SEC-008: HSTS for consistency with the other API entrypoints (non-Development only).
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseRouting();

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

app.UseAuthentication();

app.UseAuthorization();

app.MapGraphQL();

app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("live")
});

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready")
});

await app.RunAsync();
