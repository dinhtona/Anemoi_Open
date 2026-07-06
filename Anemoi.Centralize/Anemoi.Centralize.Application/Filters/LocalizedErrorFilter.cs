using System.Threading.Tasks;
using Anemoi.BuildingBlock.Application.Errors;
using Anemoi.BuildingBlock.Application.Resources;
using Anemoi.BuildingBlock.Application.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Localization;

namespace Anemoi.Centralize.Application.Filters;

public sealed class LocalizedErrorFilter(IStringLocalizer<SharedResource> localizer) : IAsyncResultFilter
{
    public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        if (context.Result is ObjectResult result)
        {
            if (result.Value is ErrorDetail errorDetail)
            {
                var translation = localizer[errorDetail.Code];
                if (!translation.ResourceNotFound)
                {
                    errorDetail.Messages = [translation.Value];
                }
            }
            else if (result.Value is ErrorDetailResponse errorDetailResponse)
            {
                if (!string.IsNullOrEmpty(errorDetailResponse.Code))
                {
                    var translation = localizer[errorDetailResponse.Code];
                    if (!translation.ResourceNotFound)
                    {
                        errorDetailResponse.Messages = [translation.Value];
                    }
                }
            }
        }
        await next();
    }
}
