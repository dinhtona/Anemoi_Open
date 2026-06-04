using Anemoi.BuildingBlock.Infrastructure.GeneralInstaller;
using Anemoi.Hr.Api.Services;
using Anemoi.Hr.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseDefaultServiceProvider((context, provider) =>
{
    provider.ValidateScopes =
    provider.ValidateOnBuild =
            context.HostingEnvironment.IsDevelopment();
});

builder.Services.InstallServicesInAssembly<IHrInfrastructureAssemblyMarker>(builder.Configuration);
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer();
builder.Services.AddHostedService<MonthlyLeaveAccrualWorker>();

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
app.MapControllers();

await app.RunAsync();
