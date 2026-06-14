using System.Net;
using System.Net.Http.Json;
using Anemoi.BuildingBlock.Domain.StronglyTypedHelper;
using Anemoi.Hr.Application.Cqrs.Commands.PayslipCommands.PublishPayslip;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Anemoi.BuildingBlock.Test;

public sealed class HrPayslipJsonBindingTests
{
    [Fact]
    public async Task PublishPayslip_Post_BindsPayslipIdFromGuidString()
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Services.ConfigureHttpJsonOptions(options =>
            options.SerializerOptions.Converters.Add(new StronglyTypedIdJsonConverterFactory()));

        await using var app = builder.Build();
        app.MapPost(
            "/api/hr/payroll/payslip/publish-payslip",
            (PublishPayslipCommand command) =>
                Results.Ok(new { payslipId = command.PayslipId.Value }));
        await app.StartAsync();

        using var client = app.GetTestClient();
        var payslipId = Guid.NewGuid();

        var response = await client.PostAsJsonAsync(
            "/api/hr/payroll/payslip/publish-payslip",
            new { payslipId = payslipId.ToString() });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<PublishPayslipBindingResponse>();
        Assert.NotNull(body);
        Assert.Equal(payslipId, body.PayslipId);
    }

    private sealed record PublishPayslipBindingResponse(Guid PayslipId);
}
