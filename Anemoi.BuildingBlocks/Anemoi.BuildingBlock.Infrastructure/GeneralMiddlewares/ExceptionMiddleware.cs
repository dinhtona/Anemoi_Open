using System;
using System.Threading.Tasks;
using Anemoi.BuildingBlock.Application.Resources;
using Anemoi.BuildingBlock.Application.Responses;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using Serilog;

namespace Anemoi.BuildingBlock.Infrastructure.GeneralMiddlewares;

public sealed class ExceptionMiddleware(
    RequestDelegate next,
    ILogger logger,
    IStringLocalizer<SharedResource> localizer)
{
    public async Task Invoke(HttpContext context)
    {
        try
        {
            await next.Invoke(context);
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error while executing a request");
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var response = context.Response;
        response.ContentType = "application/json";
        response.StatusCode = StatusCodes.Status500InternalServerError;
        if (exception is ValidationException validationException)
        {
            response.StatusCode = StatusCodes.Status400BadRequest;
            foreach (var error in validationException.Errors)
            {
                var key = GetValidationResourceKey(error.ErrorCode, error.ErrorMessage);
                error.ErrorMessage = localizer[key].Value;
            }
            await response.WriteAsJsonAsync(new { validationException.Errors });
            return;
        }

        await response.WriteAsJsonAsync(new ErrorDetailResponse
        {
            Code = "500",
            Messages = [localizer["UnhandledError"].Value]
        });
    }

    private static string GetValidationResourceKey(string errorCode, string errorMessage)
    {
        if (errorMessage.StartsWith("VAL_", StringComparison.Ordinal))
            return errorMessage;

        return errorCode switch
        {
            "NotEmptyValidator" => "VAL_REQUIRED",
            "NotNullValidator" => "VAL_REQUIRED",
            "EmailValidator" => "VAL_EMAIL_INVALID",
            "MaximumLengthValidator" => "VAL_MAX_LENGTH",
            "MinimumLengthValidator" => "VAL_MIN_LENGTH",
            "RegularExpressionValidator" => "VAL_FORMAT_INVALID",
            "PredicateValidator" => "VAL_INVALID",
            _ => "VAL_INVALID"
        };
    }
}
