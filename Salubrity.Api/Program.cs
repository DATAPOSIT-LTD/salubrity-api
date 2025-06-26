using System.Text;
using Hellang.Middleware.ProblemDetails;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Salubrity.Application;
using Salubrity.Application.Interfaces.Security;
using Salubrity.Infrastructure;
using Salubrity.Infrastructure.Security;
using Salubrity.Shared;
using Salubrity.Shared.Exceptions;
using Salubrity.Shared.Extensions;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// -------------------- Logging --------------------
builder.Host.UseSerilog((context, services, config) =>
{
    config.ReadFrom.Configuration(context.Configuration)
          .WriteTo.Console();
});

// -------------------- DI --------------------
builder.Services.AddApplicationLayer();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddSharedServices(builder.Configuration);

// Register your key provider and JWT service
builder.Services.AddSingleton<IKeyProvider, RsaKeyProvider>(); // or however yours is implemented
builder.Services.AddScoped<IJwtService, JwtService>();

// -------------------- Auth (RSA-based) --------------------
var sp = builder.Services.BuildServiceProvider();
var keyProvider = sp.GetRequiredService<IKeyProvider>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = "Salubrity",
            ValidAudience = "SalubrityClient",
            IssuerSigningKey = keyProvider.GetPublicKey()
        };
    });

// -------------------- API + Swagger --------------------
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

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

// -------------------- ProblemDetails --------------------
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

    options.IncludeExceptionDetails = (ctx, _) =>
        ctx.RequestServices.GetRequiredService<IHostEnvironment>().IsDevelopment();
});

// -------------------- CORS --------------------
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader());
});

// -------------------- Health --------------------
builder.Services.AddHealthChecks();

// -------------------- App --------------------
var app = builder.Build();

app.UseProblemDetails();

//if (app.Environment.IsDevelopment())
//{
    app.UseSwagger();
    app.UseSwaggerUI();
//}

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
