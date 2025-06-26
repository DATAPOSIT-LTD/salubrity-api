using Hellang.Middleware.ProblemDetails;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.IdentityModel.Tokens;
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


// ✅ Configure Auth *before* middleware pipeline is built
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = "Salubrity",
            ValidAudience = "SalubrityClient",
            //IssuerSigningKey = keyProvider.GetPublicKey()
        };
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
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Salubrity API",
        Version = "v1"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your JWT token like: Bearer {your token}"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});


builder.Services.AddProblemDetails(options =>
{
    options.Map<BaseAppException>(ex =>
    {
        var pd = new ProblemDetails
        {
            Title = "Application Error",
            Detail = ex.Message,
            Status = ex.StatusCode,
            Type = $"https://httpstatuses.com/{ex.StatusCode}"
        };

        if (!string.IsNullOrWhiteSpace(ex.ErrorCode))
        {
            pd.Extensions["errorCode"] = ex.ErrorCode;
        }

        if (ex.Errors is not null && ex.Errors.Any())
        {
            pd.Extensions["errors"] = ex.Errors;
        }

        return pd;
    });

    options.Map<UnauthorizedException>(ex => new ProblemDetails
    {
        Title = "Unauthorized",
        Detail = ex.Message,
        Status = StatusCodes.Status401Unauthorized,
        Type = "https://tools.ietf.org/html/rfc7235#section-3.1"
    });

    options.MapToStatusCode<NotFoundException>(StatusCodes.Status404NotFound);
    options.MapToStatusCode<ForbiddenException>(StatusCodes.Status403Forbidden);

    options.IncludeExceptionDetails = (ctx, _) => true; // Optional: or enable for development
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

builder.WebHost.ConfigureKestrel(serverOptions =>
{
    //serverOptions.ListenAnyIP(80); 

    // Optional: If you also want HTTPS support
    // serverOptions.ListenAnyIP(443, listenOptions => {
    //     listenOptions.UseHttps();
    // });
});


// Build pipeline
var app = builder.Build();

app.UseProblemDetails();

if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseCors("AllowAll");

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

app.MapGet("/", context =>
{
    context.Response.Redirect("/swagger");
    return Task.CompletedTask;
});


app.Run();
