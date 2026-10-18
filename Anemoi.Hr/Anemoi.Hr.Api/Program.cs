using System.Reflection;
using Anemoi.BuildingBlock.Infrastructure.GeneralInstaller;
using Anemoi.BuildingBlock.Infrastructure.GeneralMiddlewares;
using Anemoi.Hr.Api.Services;
using Anemoi.Hr.Infrastructure;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Anemoi.Hr.Infrastructure.SeedData;
using Anemoi.BuildingBlock.Application.Configurations;
using Anemoi.BuildingBlock.Application.Helpers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseDefaultServiceProvider((context, provider) =>
{
    provider.ValidateScopes =
    provider.ValidateOnBuild =
            context.HostingEnvironment.IsDevelopment();
});

builder.Configuration
    .AddUserSecrets(Assembly.GetExecutingAssembly())
    .AddEnvironmentVariables()
    .AddJsonFile("serilogConfiguration.json");

builder.Host.UseSerilog((host, configuration) => configuration.Enrich
    .FromLogContext()
    .ReadFrom.Configuration(host.Configuration)
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .MinimumLevel.Override("System", LogEventLevel.Information));

builder.Host.ConfigureServices((context, services) =>
{
    services.InstallServicesInAssembly<IHrInfrastructureAssemblyMarker>(context.Configuration);
    services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        var jwtSettings = context.Configuration.GetSection(nameof(JwtSetting)).Get<JwtSetting>()!;
        var publicKeyPath = JwtSecurity.ResolveKeyPath(jwtSettings.PublicKeyPath,
            JwtSecurity.DevelopmentPublicKeyRelativePath);
        JwtSecurity.EnsureDevelopmentPublicKeyExists(publicKeyPath);
        var publicSigningCredential = JwtSecurity.GetPublicSigningCredential(publicKeyPath);

        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = publicSigningCredential,
            ValidateAudience = true,
            ValidAudience = jwtSettings.Audience,
            ValidateLifetime = true,
            ValidateIssuer = true,
            ValidIssuer = jwtSettings.Issuer,
            ClockSkew = TimeSpan.Zero
        };
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = messageReceivedContext =>
            {
                if (messageReceivedContext.Request.Cookies.ContainsKey("access_token"))
                {
                    messageReceivedContext.Token = messageReceivedContext.Request.Cookies["access_token"];
                }
                return Task.CompletedTask;
            }
        };
    });
    services.AddLocalization();
    services.AddHostedService<MonthlyLeaveAccrualWorker>();
    services.AddHostedService<DepartmentTransferWorker>();
    services.AddHostedService<ContractExpirationWorker>();

    services.AddHealthChecks()
        .AddCheck("self", () => HealthCheckResult.Healthy(), tags: ["live"])
        .AddCheck("rabbitmq", () =>
        {
            var host = context.Configuration.GetValue<string>("MassTransitSetting:Host") ?? "localhost";
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
        }, tags: ["ready"])
        .AddCheck("postgres", () =>
        {
            // FUNC-007: /health/ready must not report ready when Postgres is unreachable.
            // (Manual Npgsql ping — same style as the other checks here; no new library.)
            var connectionString = context.Configuration.GetValue<string>("PostgresDbSetting:ConnectionString");
            if (string.IsNullOrWhiteSpace(connectionString))
                return HealthCheckResult.Unhealthy("PostgresDbSetting:ConnectionString is not configured");
            try
            {
                using var connection = new Npgsql.NpgsqlConnection(connectionString);
                connection.Open();
                return HealthCheckResult.Healthy();
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy("Postgres unreachable", ex);
            }
        }, tags: ["ready"]);
});

var app = builder.Build();

// SEC-008: enforce HTTPS at the app layer when not in Development
// (Caddy/Cloudflare terminate TLS in front, this is defense-in-depth).
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
    app.UseHttpsRedirection();
}

var supportedCultures = new[] { "en-US", "vi-VN" };
var localizationOptions = new RequestLocalizationOptions()
    .SetDefaultCulture("vi-VN")
    .AddSupportedCultures(supportedCultures)
    .AddSupportedUICultures(supportedCultures);

localizationOptions.ApplyCurrentCultureToResponseHeaders = true;
app.UseRequestLocalization(localizationOptions);

app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<ExceptionMiddleware>();

app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("live")
});

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready")
});

app.MapControllers();

await Anemoi.BuildingBlock.Infrastructure.RunSqlMigration.MigrationDatabase.MigrationDatabaseAsync<Anemoi.Hr.Infrastructure.Persistence.HrDbContext>(app);

// FUNC-004 / ADR-029: required workflow definitions are definition-bound
// (hierarchy fallback is forbidden in production), so the idempotent definition
// seed runs in EVERY environment. Demo data stays Development-only.
using (var serviceScope = app.Services.CreateScope())
    await HrWorkflowDefinitionSeedData.SeedAsync(serviceScope);

if (app.Environment.IsDevelopment())
{
    using var devScope = app.Services.CreateScope();
    await HrDevSeedData.SeedAsync(devScope);
}

await app.RunAsync();
