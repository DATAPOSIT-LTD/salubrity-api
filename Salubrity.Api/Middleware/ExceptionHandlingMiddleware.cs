using System.Net;
using System.Text.Json;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Salubrity.Shared.Exceptions;
using Salubrity.Shared.Responses;
using FluentValidationException = FluentValidation.ValidationException;

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

            context.Response.StatusCode = (int)HttpStatusCode.UnprocessableEntity;
            context.Response.ContentType = "application/json";

            var errorList = ex.Errors
                .Select(e => new ErrorDetail
                {
                    Field = e.PropertyName,
                    Message = e.ErrorMessage
                })
                .ToList();

            var response = ApiResponse<string>.CreateFailure("Validation failed.", errorList);
            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
        catch (Shared.Exceptions.ValidationException ex) // Your custom one
        {
            _logger.LogWarning("Domain validation failed: {Errors}", ex.Errors);

            context.Response.StatusCode = (int)HttpStatusCode.UnprocessableEntity;
            context.Response.ContentType = "application/json";

            var errorList = ex.Errors
                .Select(e => new ErrorDetail
                {
                    Field = "general", // or customize as needed
                    Message = e
                })
                .ToList();

            var response = ApiResponse<string>.CreateFailure("Validation failed.", errorList);
            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
        catch (BaseAppException ex)
        {
            _logger.LogWarning("Handled AppException: {Message}", ex.Message);

            context.Response.StatusCode = ex.StatusCode;
            context.Response.ContentType = "application/json";

            var errorList = ex.Errors?.Select(e => new ErrorDetail
            {
                Field = "general",
                Message = e
            }).ToList();

            var response = ApiResponse<string>.CreateFailure(ex.Message, errorList);
            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception");

            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            context.Response.ContentType = "application/json";

            var response = ApiResponse<string>.CreateFailure("An unexpected error occurred.");
            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
    }
}
