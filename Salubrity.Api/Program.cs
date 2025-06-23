using Hellang.Middleware.ProblemDetails;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.OpenApi.Models;
using Salubrity.Application;
using Salubrity.Application.Interfaces;
using Salubrity.Application.Services;
using Salubrity.Infrastructure;
using Salubrity.Infrastructure.Seeders;
using Salubrity.Shared;
using Salubrity.Shared.Exceptions;
using Salubrity.Shared.Extensions;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Logging
builder.Host.UseSerilog((context, services, config) =>
{
    config.ReadFrom.Configuration(context.Configuration)
          .WriteTo.Console();
});

// Framework services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// API Versioning
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = new UrlSegmentApiVersionReader();
});

builder.Services.AddVersionedApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});


builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Salubrity API", Version = "v1" });
});

// Swagger (just declared once)
builder.Services.AddSwaggerGen();


builder.Services.AddProblemDetails(options =>
{
    options.Map<BaseAppException>(ex => new ProblemDetails
    {
        Title = "Application Error",
        Detail = ex.Message,
        Status = StatusCodes.Status400BadRequest,
        Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1"
    });

    options.Map<UnauthorizedException>(ex => new ProblemDetails
    {
        Title = "Unauthorized",
        Detail = ex.Message,
        Status = StatusCodes.Status401Unauthorized,
        Type = "https://tools.ietf.org/html/rfc7235#section-3.1"
    });

    options.Map<ValidationException>(ex => new ProblemDetails
    {
        Title = "Validation Failed",
        Detail = "One or more validation errors occurred.",
        Status = StatusCodes.Status422UnprocessableEntity,
        Type = "https://tools.ietf.org/html/rfc4918#section-11.2",
        Extensions = { ["errors"] = ex.Errors } 
    });

    options.MapToStatusCode<NotFoundException>(StatusCodes.Status404NotFound);
    options.MapToStatusCode<ForbiddenException>(StatusCodes.Status403Forbidden);

    options.IncludeExceptionDetails = (ctx, _) => false;
});

// DI layers
builder.Services.AddApplicationLayer();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddSharedServices(builder.Configuration);

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader());
});

// Health checks
builder.Services.AddHealthChecks();

// Build pipeline
var app = builder.Build();

app.UseProblemDetails();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
