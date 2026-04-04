using System.Reflection;
using Anemoi.BuildingBlock.Infrastructure.GeneralInstaller;
using Anemoi.BuildingBlock.Infrastructure.GeneralMiddlewares;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.RateLimiting;
using Serilog;
using Serilog.Events;
using Anemoi.Centralize.Infrastructure;

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
    services.InstallServicesInAssembly<ICentralizeInfrastructureAssemblyMarker>(context.Configuration);
    services.AddHttpLogging(options
        => options.LoggingFields = HttpLoggingFields.All);
    services.AddRateLimiter(options =>
    {
        // Strict limit for authentication endpoints (login, register, password reset)
        // Protects against brute-force and credential-stuffing attacks
        options.AddSlidingWindowLimiter("auth-limit", opt =>
        {
            opt.Window = TimeSpan.FromSeconds(60);
            opt.SegmentsPerWindow = 6;
            opt.PermitLimit = 5;
            opt.QueueLimit = 0;
        });

        // General limit for all other API endpoints
        options.AddFixedWindowLimiter("general-limit", opt =>
        {
            opt.Window = TimeSpan.FromSeconds(60);
            opt.PermitLimit = 100;
            opt.QueueLimit = 10;
        });

        options.RejectionStatusCode = 429;
        options.OnRejected = async (ctx, token) =>
        {
            ctx.HttpContext.Response.StatusCode = 429;
            ctx.HttpContext.Response.ContentType = "application/json";
            await ctx.HttpContext.Response.WriteAsync(
                "{\"error\":\"Too many requests. Please try again later.\"}",
                cancellationToken: token);
        };
    });
});

var app = builder.Build();

if (builder.Environment.IsDevelopment())
    app.UseDeveloperExceptionPage();

if (builder.Environment.IsDevelopment() || builder.Environment.IsStaging())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("CorsPolicy");

app.UseRateLimiter();

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

// Setup Localization
var supportedCultures = new[] { "en-US", "vi-VN" };
var localizationOptions = new RequestLocalizationOptions()
    .SetDefaultCulture("vi-VN")
    .AddSupportedCultures(supportedCultures)
    .AddSupportedUICultures(supportedCultures);

// Automatically extract language from Accept-Language header
localizationOptions.ApplyCurrentCultureToResponseHeaders = true;
app.UseRequestLocalization(localizationOptions);

app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<ExceptionMiddleware>();

app.MapControllers();

await app.RunAsync();