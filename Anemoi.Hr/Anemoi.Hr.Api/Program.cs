using System.Reflection;
using Anemoi.BuildingBlock.Infrastructure.GeneralInstaller;
using Anemoi.BuildingBlock.Infrastructure.GeneralMiddlewares;
using Anemoi.Hr.Api.Services;
using Anemoi.Hr.Infrastructure;
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
        var jwtSettings = context.Configuration.GetSection(nameof(JwtSetting)).Get<JwtSetting>();
        var publicKeyPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, jwtSettings.PublicKeyPath);
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
});

var app = builder.Build();

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
app.MapControllers();

await Anemoi.BuildingBlock.Infrastructure.RunSqlMigration.MigrationDatabase.MigrationDatabaseAsync<Anemoi.Hr.Infrastructure.Persistence.HrDbContext>(app);

if (app.Environment.IsDevelopment())
{
    using var serviceScope = app.Services.CreateScope();
    await HrDevSeedData.SeedAsync(serviceScope);
}

await app.RunAsync();
