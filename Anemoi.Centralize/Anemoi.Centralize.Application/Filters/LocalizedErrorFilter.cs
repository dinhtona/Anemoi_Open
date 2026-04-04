using System.Threading.Tasks;
using Anemoi.BuildingBlock.Application.Errors;
using Anemoi.BuildingBlock.Application.Resources;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Localization;

namespace Anemoi.Centralize.Application.Filters;

public sealed class LocalizedErrorFilter(IStringLocalizer<SharedResource> localizer) : IAsyncResultFilter
{
    public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        if (context.Result is ObjectResult result && result.Value is ErrorDetail errorDetail)
        {
            var translation = localizer[errorDetail.Code];
            if (!translation.ResourceNotFound)
            {
                errorDetail.Messages = [translation.Value];
            }
        }
        await next();
    }
}
