using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using Million.Application.DTOs;
using Million.Application.DTOs.Auth;
using Million.Application.Interfaces;
using Million.Application.Services;
using Million.Application.Validation;
using Million.Infrastructure.Config;
using Million.Infrastructure.Migrations;
using Million.Infrastructure.Persistence;
using Million.Infrastructure.Repositories;
using Million.Infrastructure.Services;
using Million.Web.Middlewares;
using Million.Domain.Exceptions;
using Million.Domain.Entities;
using Serilog;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .Enrich.FromLogContext()
    .Enrich.WithEnvironmentName()
    .WriteTo.Console()
    .CreateLogger();
builder.Host.UseSerilog();

builder.Services.Configure<MongoOptions>(opts =>
{
    opts.Uri = Environment.GetEnvironmentVariable("MONGO_URI") ?? "mongodb://localhost:27017";
    opts.Database = Environment.GetEnvironmentVariable("MONGO_DB") ?? "million";
});

// Configure rate limiting
builder.Services.AddAdvancedRateLimiting(options =>
{
    options.DefaultLimitPerMinute = int.TryParse(Environment.GetEnvironmentVariable("RATE_LIMIT_PERMINUTE"), out var limit) ? limit : 120;
    options.BurstLimit = int.TryParse(Environment.GetEnvironmentVariable("RATE_LIMIT_BURST"), out var burst) ? burst : 200;
    options.EnableBurst = Environment.GetEnvironmentVariable("RATE_LIMIT_ENABLE_BURST") != "false";
});

var corsOrigins = (Environment.GetEnvironmentVariable("CORS_ORIGINS") ?? "http://localhost:3000")
    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
builder.Services.AddCors(options =>
{
    options.AddPolicy("Allowlist", policy =>
    {
        policy.WithOrigins(corsOrigins).AllowAnyHeader().AllowAnyMethod();
    });
});

builder.Services.AddMemoryCache();
builder.Services.AddSingleton<SimpleRateLimiter>(); // Keep for backward compatibility

builder.Services.AddControllers().AddJsonOptions(o =>
{
    o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<PropertyListQueryValidator>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Million Properties API", Version = "v1", Description = "Luxury real estate demo with Vercel Blob image support" });
});

builder.Services.AddSingleton<MongoContext>();
builder.Services.AddScoped<IPropertyRepository, PropertyRepository>();
builder.Services.AddScoped<IPropertyService, PropertyService>();

// New authentication services
builder.Services.AddScoped<IOwnerRepository, OwnerRepository>();
builder.Services.AddScoped<IOwnerSessionRepository, OwnerSessionRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IPasswordService, PasswordService>();
builder.Services.AddScoped<IExplainService, ExplainService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var ctx = scope.ServiceProvider.GetRequiredService<MongoContext>();
    await EnsureIndexes.RunAsync(ctx, CancellationToken.None);
}

// Middleware pipeline
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseStructuredLogging();

// Use conditional ProblemDetailsMiddleware based on configuration
var skipProblemDetails = app.Configuration.GetValue<bool>("SkipProblemDetailsMiddleware", false);

if (!skipProblemDetails)
{
    Console.WriteLine("✅ Adding ProblemDetailsMiddleware");
    app.UseMiddleware<ProblemDetailsMiddleware>();
}
else
{
    Console.WriteLine("🚫 Skipping ProblemDetailsMiddleware (configured to skip)");
}

app.UseAdvancedRateLimiting();
// JWT authentication middleware - will be applied selectively to protected endpoints
app.UseSerilogRequestLogging();

app.UseCors("Allowlist");

app.MapGet("/health/live", () => Results.Ok(new { status = "ok" }));
app.MapGet("/health/ready", () => Results.Ok(new { status = "ready" }));

// Public endpoints (no authentication required)
app.MapGet("/properties", async (
    [AsParameters] PropertyListQuery query,
    IValidator<PropertyListQuery> validator,
    HttpContext http,
    IPropertyService service,
    CancellationToken ct) =>
{
    var result = await validator.ValidateAsync(query, ct);
    if (!result.IsValid)
    {
        var detail = string.Join("; ", result.Errors.Select(e => e.ErrorMessage));
        await ProblemDetailsMiddleware.WriteProblemDetails(http, StatusCodes.Status400BadRequest, "Validation Failed", detail);
        return Results.ValidationProblem(result.ToDictionary());
    }

    var data = await service.GetPropertiesAsync(query, ct);
    return Results.Ok(data);
}).WithTags("Properties");

app.MapGet("/properties/{id}", async (string id, IPropertyService service, HttpContext http, CancellationToken ct) =>
{
    var item = await service.GetByIdAsync(id, ct);
    if (item is null)
    {
        await ProblemDetailsMiddleware.WriteProblemDetails(http, StatusCodes.Status404NotFound, "Resource Not Found", $"Property {id} not found");
        return Results.NotFound();
    }
    return Results.Ok(item);
}).WithTags("Properties");





// Explain endpoint for query optimization
app.MapGet("/properties/explain", async (
    [AsParameters] PropertyListQuery query,
    IExplainService explainService,
    CancellationToken ct) =>
{
    var explain = await explainService.ExplainPropertyListingPipelineAsync(query, ct);
    return Results.Ok(new { explain });
}).WithTags("Properties");

// Authentication endpoints (no authentication required)
app.MapPost("/auth/owner/login", async (
    LoginRequest request,
    IValidator<LoginRequest> validator,
    IAuthService authService,
    HttpContext http,
    CancellationToken ct) =>
{
    var result = await validator.ValidateAsync(request, ct);
    if (!result.IsValid)
    {
        var detail = string.Join("; ", result.Errors.Select(e => e.ErrorMessage));
        await ProblemDetailsMiddleware.WriteProblemDetails(http, StatusCodes.Status400BadRequest, "Validation Failed", detail);
        return Results.ValidationProblem(result.ToDictionary());
    }

    try
    {
        var ip = http.Connection.RemoteIpAddress?.ToString();
        var userAgent = http.Request.Headers.UserAgent.ToString();
        var response = await authService.LoginAsync(request, ip, userAgent, ct);
        return Results.Ok(response);
    }
    catch (InvalidCredentialsException)
    {
        await ProblemDetailsMiddleware.WriteProblemDetails(http, StatusCodes.Status401Unauthorized, "Invalid Credentials", "Email or password is incorrect");
        return Results.Unauthorized();
    }
    catch (AccountLockedException ex)
    {
        await ProblemDetailsMiddleware.WriteProblemDetails(http, StatusCodes.Status429TooManyRequests, "Account Locked", ex.Message);
        return Results.StatusCode(StatusCodes.Status429TooManyRequests);
    }
}).WithTags("Authentication");

app.MapPost("/auth/owner/refresh", async (
    RefreshRequest request,
    IValidator<RefreshRequest> validator,
    IAuthService authService,
    HttpContext http,
    CancellationToken ct) =>
{
    var result = await validator.ValidateAsync(request, ct);
    if (!result.IsValid)
    {
        var detail = string.Join("; ", result.Errors.Select(e => e.ErrorMessage));
        await ProblemDetailsMiddleware.WriteProblemDetails(http, StatusCodes.Status400BadRequest, "Validation Failed", detail);
        return Results.ValidationProblem(result.ToDictionary());
    }

    try
    {
        var response = await authService.RefreshAsync(request, null, null, ct);
        return Results.Ok(response);
    }
    catch (InvalidTokenException)
    {
        await ProblemDetailsMiddleware.WriteProblemDetails(http, StatusCodes.Status401Unauthorized, "Invalid Refresh Token", "The refresh token is invalid or expired");
        return Results.Unauthorized();
    }
}).WithTags("Authentication");

app.MapPost("/auth/owner/logout", async (
    RefreshRequest request,
    IValidator<RefreshRequest> validator,
    IAuthService authService,
    HttpContext http,
    CancellationToken ct) =>
{
    var result = await validator.ValidateAsync(request, ct);
    if (!result.IsValid)
    {
        var detail = string.Join("; ", result.Errors.Select(e => e.ErrorMessage));
        await ProblemDetailsMiddleware.WriteProblemDetails(http, StatusCodes.Status400BadRequest, "Validation Failed", detail);
        return Results.ValidationProblem(result.ToDictionary());
    }

    try
    {
        await authService.LogoutAsync(request.RefreshToken, ct);
        return Results.Ok(new { message = "Logged out successfully" });
    }
    catch (InvalidTokenException)
    {
        await ProblemDetailsMiddleware.WriteProblemDetails(http, StatusCodes.Status401Unauthorized, "Invalid Refresh Token", "The refresh token is invalid or expired");
        return Results.Unauthorized();
    }
}).WithTags("Authentication");

// Apply JWT authentication middleware to protected endpoints (skip in testing if configured)
var skipJwtAuth = app.Configuration.GetValue<bool>("SkipJwtAuthMiddleware", false);

if (!skipJwtAuth)
{
    Console.WriteLine("✅ Adding JwtAuthMiddleware");
    app.UseJwtAuth();
}
else
{
    Console.WriteLine("🚫 Skipping JwtAuthMiddleware (configured to skip)");
}

// Protected endpoints (authentication required)
app.MapPost("/properties", async (
    CreatePropertyRequest request,
    IValidator<CreatePropertyRequest> validator,
    HttpContext http,
    IPropertyService service,
    CancellationToken ct) =>
{
    var result = await validator.ValidateAsync(request, ct);
    if (!result.IsValid)
    {
        var detail = string.Join("; ", result.Errors.Select(e => e.ErrorMessage));
        await ProblemDetailsMiddleware.WriteProblemDetails(http, StatusCodes.Status400BadRequest, "Validation Failed", detail);
        return Results.ValidationProblem(result.ToDictionary());
    }

    var createdProperty = await service.CreatePropertyAsync(request, ct);
    return Results.Created($"/properties/{createdProperty.Id}", createdProperty);
}).WithTags("Properties");

app.MapPut("/properties/{id}", async (
    string id,
    UpdatePropertyRequest request,
    IValidator<UpdatePropertyRequest> validator,
    HttpContext http,
    IPropertyService service,
    CancellationToken ct) =>
{
    var result = await validator.ValidateAsync(request, ct);
    if (!result.IsValid)
    {
        var detail = string.Join("; ", result.Errors.Select(e => e.ErrorMessage));
        await ProblemDetailsMiddleware.WriteProblemDetails(http, StatusCodes.Status400BadRequest, "Validation Failed", detail);
        return Results.ValidationProblem(result.ToDictionary());
    }

    try
    {
        var updatedProperty = await service.UpdatePropertyAsync(id, request, ct);
        return Results.Ok(updatedProperty);
    }
    catch (PropertyNotFoundException)
    {
        await ProblemDetailsMiddleware.WriteProblemDetails(http, StatusCodes.Status404NotFound, "Resource Not Found", $"Property {id} not found");
        return Results.NotFound();
    }
}).WithTags("Properties");

app.MapDelete("/properties/{id}", async (
    string id,
    HttpContext http,
    IPropertyService service,
    CancellationToken ct) =>
{
    try
    {
        var deleted = await service.DeletePropertyAsync(id, ct);
        return Results.NoContent();
    }
    catch (PropertyNotFoundException)
    {
        await ProblemDetailsMiddleware.WriteProblemDetails(http, StatusCodes.Status404NotFound, "Resource Not Found", $"Property {id} not found");
        return Results.NotFound();
    }
}).WithTags("Properties");

app.MapPatch("/properties/{id}/activate", async (
    string id,
    HttpContext http,
    IPropertyService service,
    CancellationToken ct) =>
{
    try
    {
        var activated = await service.ActivatePropertyAsync(id, ct);
        return Results.Ok(new { message = "Property activated successfully" });
    }
    catch (PropertyNotFoundException)
    {
        await ProblemDetailsMiddleware.WriteProblemDetails(http, StatusCodes.Status404NotFound, "Resource Not Found", $"Property {id} not found");
        return Results.NotFound();
    }
    catch (PropertyAlreadyActiveException ex)
    {
        await ProblemDetailsMiddleware.WriteProblemDetails(http, StatusCodes.Status409Conflict, "Property Already Active", ex.Message);
        return Results.Conflict();
    }
}).WithTags("Properties");

app.MapPatch("/properties/{id}/deactivate", async (
    string id,
    HttpContext http,
    IPropertyService service,
    CancellationToken ct) =>
{
    try
    {
        var deactivated = await service.DeactivatePropertyAsync(id, ct);
        return Results.Ok(new { message = "Property deactivated successfully" });
    }
    catch (PropertyNotFoundException)
    {
        await ProblemDetailsMiddleware.WriteProblemDetails(http, StatusCodes.Status404NotFound, "Resource Not Found", $"Property {id} not found");
        return Results.NotFound();
    }
    catch (PropertyAlreadyInactiveException ex)
    {
        await ProblemDetailsMiddleware.WriteProblemDetails(http, StatusCodes.Status409Conflict, "Property Already Inactive", ex.Message);
        return Results.Conflict();
    }
}).WithTags("Properties");

// Protected media management endpoints
app.MapGet("/properties/{id}/media", async (
    string id,
    [AsParameters] MediaQueryDto query,
    IPropertyRepository repository,
    HttpContext http,
    CancellationToken ct) =>
{
    var media = await repository.GetMediaAsync(id, query, ct);
    return Results.Ok(media);
}).WithTags("Properties");

app.MapPatch("/properties/{id}/media", async (
    string id,
    MediaPatchDto request,
    IPropertyRepository repository,
    HttpContext http,
    CancellationToken ct) =>
{
    var success = await repository.UpdateMediaAsync(id, request, ct);
    if (success)
        return Results.Ok(new { message = "Media updated successfully" });
    return Results.NotFound();
}).WithTags("Properties");

app.MapPost("/properties/{id}/traces", async (
    string id,
    PropertyTraceDto request,
    IPropertyRepository repository,
    HttpContext http,
    CancellationToken ct) =>
{
    var trace = PropertyTrace.Create(request.DateSale, request.Name, request.Value, request.Tax);
    var success = await repository.AddTraceAsync(id, trace, ct);
    if (success)
        return Results.Ok(new { message = "Trace added successfully" });
    return Results.NotFound();
}).WithTags("Properties");

// Admin endpoints for managing owners and sessions
app.MapGet("/admin/owners", async (
    IAuthService authService,
    HttpContext http,
    CancellationToken ct) =>
{
    try
    {
        var owners = await authService.GetOwnersAsync(null, 1, 1000, ct);
        return Results.Ok(owners);
    }
    catch (InsufficientPermissionsException)
    {
        await ProblemDetailsMiddleware.WriteProblemDetails(http, StatusCodes.Status403Forbidden, "Insufficient Permissions", "Admin access required");
        return Results.Forbid();
    }
}).WithTags("Admin");

app.MapGet("/admin/sessions", async (
    string ownerId,
    IAuthService authService,
    HttpContext http,
    CancellationToken ct) =>
{
    try
    {
        var sessions = await authService.GetOwnerSessionsAsync(ownerId, ct);
        return Results.Ok(sessions);
    }
    catch (InsufficientPermissionsException)
    {
        await ProblemDetailsMiddleware.WriteProblemDetails(http, StatusCodes.Status403Forbidden, "Insufficient Permissions", "Admin access required");
        return Results.Forbid();
    }
}).WithTags("Admin");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Disable DeveloperExceptionPage completely to avoid middleware conflicts in testing
// app.UseDeveloperExceptionPage();

app.Run();

// Public class for testing
public partial class Program { }
