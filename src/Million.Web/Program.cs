using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using Million.Application.DTOs;
using Million.Application.Interfaces;
using Million.Application.Services;
using Million.Application.Validation;
using Million.Infrastructure.Config;
using Million.Infrastructure.Migrations;
using Million.Infrastructure.Persistence;
using Million.Infrastructure.Repositories;
using Million.Web.Middlewares;
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
builder.Services.AddSingleton<SimpleRateLimiter>();

builder.Services.AddControllers().AddJsonOptions(o =>
{
    o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<PropertyListQueryValidator>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Million Properties API", Version = "v1", Description = "Luxury real estate demo" });
});

builder.Services.AddSingleton<MongoContext>();
builder.Services.AddScoped<IPropertyRepository, PropertyRepository>();
builder.Services.AddScoped<IPropertyService, PropertyService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var ctx = scope.ServiceProvider.GetRequiredService<MongoContext>();
    await EnsureIndexes.RunAsync(ctx, CancellationToken.None);
}

app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<ProblemDetailsMiddleware>();
app.UseSerilogRequestLogging();

app.UseCors("Allowlist");

app.MapGet("/health/live", () => Results.Ok(new { status = "ok" }));
app.MapGet("/health/ready", () => Results.Ok(new { status = "ready" }));

app.MapGet("/properties", async (
    [AsParameters] PropertyListQuery query,
    IValidator<PropertyListQuery> validator,
    SimpleRateLimiter limiter,
    HttpContext http,
    IPropertyService service,
    CancellationToken ct) =>
{
    var clientIp = http.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    var limitPerMinute = int.TryParse(Environment.GetEnvironmentVariable("RATE_LIMIT_PERMINUTE"), out var c) ? c : 120;
    if (!limiter.Allow(clientIp, limitPerMinute))
    {
        http.Response.Headers["Retry-After"] = "60";
        await ProblemDetailsMiddleware.WriteProblemDetails(http, StatusCodes.Status429TooManyRequests, "Too Many Requests");
        return Results.StatusCode(StatusCodes.Status429TooManyRequests);
    }

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

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.Run();
