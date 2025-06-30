using System.Net;
using System.Text.Json;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Salubrity.Shared.Exceptions;
using Salubrity.Shared.Responses;
using FluentValidationException = FluentValidation.ValidationException;
using SharedValidationException = Salubrity.Shared.Exceptions.ValidationException;

namespace Salubrity.Api.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (FluentValidationException ex)
        {
            _logger.LogWarning("FluentValidation failed: {Errors}", ex.Errors);

            var errors = ex.Errors.Select(e => new ErrorDetail
            {
                Field = e.PropertyName,
                Message = e.ErrorMessage
            });

            await WriteResponse(context, HttpStatusCode.UnprocessableEntity, "Validation failed.", errors);
        }
        catch (SharedValidationException ex)
        {
            _logger.LogWarning("Domain validation failed: {Errors}", ex.Errors);

            var errors = ex.Errors.Select(e => new ErrorDetail
            {
                Field = "general",
                Message = e
            });

            await WriteResponse(context, HttpStatusCode.UnprocessableEntity, "Validation failed.", errors);
        }
        catch (BaseAppException ex)
        {
            _logger.LogWarning("Handled AppException: {Message}", ex.Message);

            var errors = ex.Errors?.Select(e => new ErrorDetail
            {
                Field = "general",
                Message = e
            });

            await WriteResponse(context, (HttpStatusCode)ex.StatusCode, ex.Message, errors);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception");

            await WriteResponse(context, HttpStatusCode.InternalServerError, "An unexpected error occurred.");
        }
    }

    private static async Task WriteResponse(HttpContext context, HttpStatusCode code, string message, IEnumerable<ErrorDetail>? errors = null)
    {
        context.Response.StatusCode = (int)code;
        context.Response.ContentType = "application/json";

        var response = ApiResponse<string>.CreateFailure(message, errors?.ToList());
        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}
