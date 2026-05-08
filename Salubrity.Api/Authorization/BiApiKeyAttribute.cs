using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Salubrity.Api.Authorization;

/// <summary>
/// Allows requests bearing a static API key in the X-BI-Key header. Used by Power BI
/// scheduled refresh, which authenticates with stored credentials rather than JWT.
/// Configure key in BI:ApiKey appsetting (or BI__ApiKey env var).
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class BiApiKeyAttribute : Attribute, IAsyncActionFilter
{
    private const string HeaderName = "X-BI-Key";

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var configured = context.HttpContext.RequestServices
            .GetService(typeof(IConfiguration)) as IConfiguration;
        var expected = configured?["BI:ApiKey"];

        if (string.IsNullOrWhiteSpace(expected))
        {
            context.Result = new ObjectResult(new { error = "BI API key is not configured." })
            { StatusCode = StatusCodes.Status503ServiceUnavailable };
            return;
        }

        if (!context.HttpContext.Request.Headers.TryGetValue(HeaderName, out var provided)
            || !string.Equals(provided.ToString(), expected, StringComparison.Ordinal))
        {
            context.Result = new UnauthorizedObjectResult(new { error = "Invalid or missing X-BI-Key header." });
            return;
        }

        await next();
    }
}
