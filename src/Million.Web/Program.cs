using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using Million.Application.DTOs;
using Million.Application.DTOs.Auth;
using Million.Application.Common;
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
using MongoDB.Bson;
using MongoDB.Driver;
using DotNetEnv;

// Load environment variables from .env file
var envPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", ".env");
Console.WriteLine($"Looking for .env file at: {envPath}");
Console.WriteLine($"File exists: {File.Exists(envPath)}");

if (File.Exists(envPath))
{
    Env.Load(envPath);
    Console.WriteLine("✅ .env file loaded successfully");
}
else
{
    Console.WriteLine("❌ .env file not found");
}

// Debug: Log environment variables
Console.WriteLine($"MONGO_URI: {Environment.GetEnvironmentVariable("MONGO_URI")}");
Console.WriteLine($"MONGO_DB: {Environment.GetEnvironmentVariable("MONGO_DB")}");
Console.WriteLine($"CORS_ORIGINS: {Environment.GetEnvironmentVariable("CORS_ORIGINS") ?? "NOT_SET"}");

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

// Debug: Log CORS configuration
Console.WriteLine($"🔒 CORS Policy 'Allowlist' configured with origins: [{string.Join(", ", corsOrigins)}]");

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

// New services for advanced functionality
builder.Services.AddScoped<IPropertyTraceRepository, PropertyTraceRepository>();
builder.Services.AddScoped<IPropertyTraceService, PropertyTraceService>();
builder.Services.AddScoped<IMediaManagementService, MediaManagementService>();
builder.Services.AddScoped<IAdvancedSearchService, AdvancedSearchService>();
builder.Services.AddScoped<IPropertyStatsService, PropertyStatsService>();
builder.Services.AddScoped<IWebhookService, WebhookService>();
builder.Services.AddScoped<IOwnerProfileService, OwnerProfileService>();

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

// Test MongoDB connection endpoint
app.MapGet("/test/mongo", async (MongoContext mongoContext) =>
{
    try
    {
        var collection = mongoContext.GetCollection<BsonDocument>("properties");
        var count = await collection.CountDocumentsAsync(FilterDefinition<BsonDocument>.Empty);
        var sample = await collection.Find(FilterDefinition<BsonDocument>.Empty).Limit(1).FirstOrDefaultAsync();

        return Results.Ok(new
        {
            success = true,
            count = count,
            sample = sample?.ToString()
        });
    }
    catch (Exception ex)
    {
        return Results.Problem($"MongoDB test failed: {ex.Message}");
    }
});

// Test properties count endpoint
app.MapGet("/test/properties-count", async (IPropertyService propertyService) =>
{
    try
    {
        var query = new PropertyListQuery { Page = 1, PageSize = 1 };
        var result = await propertyService.GetPropertiesAsync(query, CancellationToken.None);

        return Results.Ok(new
        {
            success = true,
            total = result.Total,
            itemsCount = result.Items.Count,
            sample = result.Items.FirstOrDefault()
        });
    }
    catch (Exception ex)
    {
        return Results.Problem($"Properties service test failed: {ex.Message}");
    }
});

// Test aggregation pipeline directly
app.MapGet("/test/aggregation", async (MongoContext mongoContext) =>
{
    try
    {
        var collection = mongoContext.GetCollection<BsonDocument>("properties");

        // Simple pipeline to test
        var pipeline = new List<BsonDocument>
        {
            new BsonDocument("$match", new BsonDocument("isActive", true)),
            new BsonDocument("$count", "total")
        };

        var result = await collection.Aggregate<BsonDocument>(pipeline).ToListAsync();
        var total = result.FirstOrDefault()?.GetValue("total").AsInt32 ?? 0;

        // Get one sample property
        var sample = await collection.Find(new BsonDocument("isActive", true)).Limit(1).FirstOrDefaultAsync();

        return Results.Ok(new
        {
            success = true,
            total = total,
            sample = sample?.ToString()
        });
    }
    catch (Exception ex)
    {
        return Results.Problem($"Aggregation test failed: {ex.Message}");
    }
});

// Test PropertyDocument mapping
app.MapGet("/test/property-document", async (MongoContext mongoContext) =>
{
    try
    {
        var collection = mongoContext.GetCollection<PropertyDocument>("properties");

        // Try to find one property using PropertyDocument
        var property = await collection.Find(x => x.IsActive == true).Limit(1).FirstOrDefaultAsync();

        if (property != null)
        {
            return Results.Ok(new
            {
                success = true,
                id = property.Id,
                name = property.Name,
                isActive = property.IsActive,
                coverType = property.Cover?.Type,
                mediaCount = property.Media?.Count ?? 0
            });
        }
        else
        {
            return Results.Ok(new { success = false, message = "No properties found" });
        }
    }
    catch (Exception ex)
    {
        return Results.Problem($"PropertyDocument test failed: {ex.Message}");
    }
});

// Test PropertyRepository pipeline
app.MapGet("/test/repository-pipeline", async (MongoContext mongoContext) =>
{
    try
    {
        var collection = mongoContext.GetCollection<BsonDocument>("properties");

        // Build the exact filter from PropertyRepository
        var builder = Builders<BsonDocument>.Filter;
        var filters = new List<FilterDefinition<BsonDocument>>();

        // Only show active properties by default
        filters.Add(builder.Eq("isActive", true));

        var filter = filters.Count > 0 ? builder.And(filters) : builder.Empty;

        // Simple pipeline to test the filter
        var pipeline = new List<BsonDocument>
        {
            new BsonDocument("$match", filter.ToBsonDocument()),
            new BsonDocument("$count", "total")
        };

        var result = await collection.Aggregate<BsonDocument>(pipeline).ToListAsync();
        var total = result.FirstOrDefault()?.GetValue("total").AsInt32 ?? 0;

        // Also test with simple find
        var findResult = await collection.Find(filter).Limit(1).FirstOrDefaultAsync();

        return Results.Ok(new
        {
            success = true,
            total = total,
            filter = filter.ToBsonDocument().ToString(),
            sample = findResult?.ToString()
        });
    }
    catch (Exception ex)
    {
        return Results.Problem($"Repository pipeline test failed: {ex.Message}");
    }
});



// Test simple filter
app.MapGet("/test/simple-filter", async (MongoContext mongoContext) =>
{
    try
    {
        var collection = mongoContext.GetCollection<BsonDocument>("properties");

        // Test different filter approaches
        var filter1 = new BsonDocument("isActive", true);
        var filter2 = Builders<BsonDocument>.Filter.Eq("isActive", true);
        var filter3 = Builders<BsonDocument>.Filter.Eq("isActive", BsonValue.Create(true));

        var count1 = await collection.CountDocumentsAsync(filter1);
        var count2 = await collection.CountDocumentsAsync(filter2);
        var count3 = await collection.CountDocumentsAsync(filter3);

        return Results.Ok(new
        {
            success = true,
            count1 = count1,
            count2 = count2,
            count3 = count3,
            filter1 = filter1.ToString(),
            filter2 = filter2.ToString(),
            filter3 = filter3.ToString()
        });
    }
    catch (Exception ex)
    {
        return Results.Problem($"Simple filter test failed: {ex.Message}");
    }
});

// Test complete pipeline step by step
app.MapGet("/test/complete-pipeline", async (MongoContext mongoContext) =>
{
    try
    {
        var collection = mongoContext.GetCollection<BsonDocument>("properties");

        // Step 1: Build filter
        var builder = Builders<BsonDocument>.Filter;
        var filters = new List<FilterDefinition<BsonDocument>>();
        filters.Add(builder.Eq("isActive", true));
        var filter = builder.And(filters);

        // Step 2: Test filter with find
        var findCount = await collection.CountDocumentsAsync(filter);

        // Step 3: Test filter with aggregation
        var pipeline = new List<BsonDocument>
        {
            new BsonDocument("$match", filter.ToBsonDocument()),
            new BsonDocument("$count", "total")
        };

        var aggResult = await collection.Aggregate<BsonDocument>(pipeline).ToListAsync();
        var aggCount = aggResult.FirstOrDefault()?.GetValue("total").AsInt32 ?? 0;

        // Step 4: Test with raw BsonDocument filter
        var rawFilter = new BsonDocument("isActive", true);
        var rawPipeline = new List<BsonDocument>
        {
            new BsonDocument("$match", rawFilter),
            new BsonDocument("$count", "total")
        };

        var rawResult = await collection.Aggregate<BsonDocument>(rawPipeline).ToListAsync();
        var rawCount = rawResult.FirstOrDefault()?.GetValue("total").AsInt32 ?? 0;

        return Results.Ok(new
        {
            success = true,
            findCount = findCount,
            aggCount = aggCount,
            rawCount = rawCount,
            filter = filter.ToBsonDocument().ToString(),
            rawFilter = rawFilter.ToString()
        });
    }
    catch (Exception ex)
    {
        return Results.Problem($"Complete pipeline test failed: {ex.Message}");
    }
});

// Test description field specifically
app.MapGet("/test/description-field", async (MongoContext mongoContext) =>
{
    try
    {
        var collection = mongoContext.GetCollection<BsonDocument>("properties");

        // Test 1: Direct find to see raw document
        var rawDoc = await collection.Find(new BsonDocument("_id", "prop-050")).FirstOrDefaultAsync();

        // Test 2: Simple projection
        var simplePipeline = new List<BsonDocument>
        {
            new BsonDocument("$match", new BsonDocument("_id", "prop-050")),
            new BsonDocument("$project", new BsonDocument
            {
                { "name", 1 },
                { "description", 1 },
                { "Description", 1 },
                { "address", 1 }
            })
        };

        var simpleResult = await collection.Aggregate<BsonDocument>(simplePipeline).FirstOrDefaultAsync();

        // Test 3: Full pipeline like the real one
        var fullPipeline = new List<BsonDocument>
        {
            new BsonDocument("$match", new BsonDocument("_id", "prop-050")),
            new BsonDocument("$project", new BsonDocument
            {
                { "name", 1 },
                { "description", 1 },
                { "address", 1 },
                { "price", 1 },
                { "year", 1 },
                { "codeInternal", 1 },
                { "ownerId", 1 },
                { "status", 1 },
                { "coverUrl", new BsonDocument("$ifNull", new BsonArray { "$cover.Url", "$cover.Poster" }) },
                { "totalImages", new BsonDocument("$size", new BsonDocument("$filter", new BsonDocument
                {
                    { "input", "$media" },
                    { "as", "m" },
                    { "cond", new BsonDocument("$eq", new BsonArray { "$$m.Type", 0 }) }
                })) },
                { "totalVideos", new BsonDocument("$size", new BsonDocument("$filter", new BsonDocument
                {
                    { "input", "$media" },
                    { "as", "m" },
                    { "cond", new BsonDocument("$eq", new BsonArray { "$$m.Type", 1 }) }
                })) }
            })
        };

        var fullResult = await collection.Aggregate<BsonDocument>(fullPipeline).FirstOrDefaultAsync();

        return Results.Ok(new
        {
            rawDocument = rawDoc?.ToString(),
            simpleProjection = simpleResult?.ToString(),
            fullProjection = fullResult?.ToString(),
            analysis = new
            {
                rawHasDescription = rawDoc?.Contains("description") ?? false,
                rawHasDescriptionPascal = rawDoc?.Contains("Description") ?? false,
                simpleHasDescription = simpleResult?.Contains("description") ?? false,
                fullHasDescription = fullResult?.Contains("description") ?? false
            }
        });
    }
    catch (Exception ex)
    {
        return Results.Problem($"Description field test failed: {ex.Message}");
    }
}).WithTags("Test");

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
        return Results.Problem(
            title: "Invalid Credentials",
            detail: "Email or password is incorrect",
            statusCode: StatusCodes.Status401Unauthorized
        );
    }
    catch (AccountLockedException ex)
    {
        return Results.Problem(
            title: "Account Locked",
            detail: ex.Message,
            statusCode: StatusCodes.Status429TooManyRequests
        );
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
        return Results.ValidationProblem(result.ToDictionary());
    }

    try
    {
        var response = await authService.RefreshAsync(request, null, null, ct);
        return Results.Ok(response);
    }
    catch (InvalidTokenException)
    {
        return Results.Problem(
            title: "Invalid Refresh Token",
            detail: "The refresh token is invalid or expired",
            statusCode: StatusCodes.Status401Unauthorized
        );
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
        return Results.ValidationProblem(result.ToDictionary());
    }

    try
    {
        await authService.LogoutAsync(request.RefreshToken, ct);
        return Results.Ok(new { message = "Logged out successfully" });
    }
    catch (InvalidTokenException)
    {
        return Results.Problem(
            title: "Invalid Refresh Token",
            detail: "The refresh token is invalid or expired",
            statusCode: StatusCodes.Status401Unauthorized
        );
    }
}).WithTags("Authentication");

// Owner profile endpoints (authentication required)
app.MapGet("/auth/owner/profile", [RequiresAuth] async (
    IAuthService authService,
    HttpContext http,
    CancellationToken ct) =>
{
    try
    {
        // Get owner ID from JWT token (already validated by middleware)
        var ownerId = http.User.FindFirst("ownerId")?.Value;
        if (string.IsNullOrEmpty(ownerId))
        {
            await ProblemDetailsMiddleware.WriteProblemDetails(http, StatusCodes.Status401Unauthorized, "Unauthorized", "Owner ID not found in token");
            return Results.Unauthorized();
        }

        var owner = await authService.GetOwnerByIdAsync(ownerId, ct);
        if (owner == null)
        {
            await ProblemDetailsMiddleware.WriteProblemDetails(http, StatusCodes.Status404NotFound, "Owner Not Found", "Owner not found");
            return Results.NotFound();
        }

        // Return owner profile without sensitive information
        return Results.Ok(new
        {
            id = owner.Id,
            fullName = owner.FullName,
            email = owner.Email,
            phone = owner.PhoneE164,
            photoUrl = owner.PhotoUrl,
            bio = owner.Bio,
            company = owner.Company,
            socialMedia = new
            {
                linkedin = owner.LinkedInUrl,
                instagram = owner.InstagramUrl,
                facebook = owner.FacebookUrl
            },
            createdAt = owner.CreatedAt,
            updatedAt = owner.UpdatedAt
        });
    }
    catch (Exception ex)
    {
        await ProblemDetailsMiddleware.WriteProblemDetails(http, StatusCodes.Status500InternalServerError, "Internal Server Error", "Failed to retrieve owner profile");
        return Results.Problem("Failed to retrieve owner profile");
    }
}).WithTags("Authentication");

app.MapPut("/auth/owner/profile", [RequiresAuth] async (
    UpdateOwnerProfileRequest request,
    IValidator<UpdateOwnerProfileRequest> validator,
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
        // Get owner ID from JWT token
        var ownerId = http.User.FindFirst("ownerId")?.Value;
        if (string.IsNullOrEmpty(ownerId))
        {
            await ProblemDetailsMiddleware.WriteProblemDetails(http, StatusCodes.Status401Unauthorized, "Unauthorized", "Owner ID not found in token");
            return Results.Unauthorized();
        }

        var updatedOwner = await authService.UpdateOwnerProfileAsync(ownerId, request, ct);
        return Results.Ok(updatedOwner);
    }
    catch (OwnerNotFoundException)
    {
        await ProblemDetailsMiddleware.WriteProblemDetails(http, StatusCodes.Status404NotFound, "Resource Not Found", "Owner not found");
        return Results.NotFound();
    }
    catch (Exception ex)
    {
        await ProblemDetailsMiddleware.WriteProblemDetails(http, StatusCodes.Status500InternalServerError, "Internal Server Error", "Failed to update owner profile");
        return Results.Problem("Failed to update owner profile");
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

// Dashboard endpoints (authentication required) - Must be after JWT middleware
app.MapGet("/auth/owner/properties", [RequiresAuth] async (
    [AsParameters] PropertyListQuery query,
    IPropertyService propertyService,
    IAuthService authService,
    HttpContext http,
    CancellationToken ct) =>
{
    try
    {
        // Get owner ID from JWT token
        var ownerId = http.User.FindFirst("ownerId")?.Value;
        if (string.IsNullOrEmpty(ownerId))
        {
            await ProblemDetailsMiddleware.WriteProblemDetails(http, StatusCodes.Status401Unauthorized, "Unauthorized", "Owner ID not found in token");
            return Results.Unauthorized();
        }

        // Filter properties by owner ID
        var ownerProperties = await propertyService.GetPropertiesByOwnerAsync(ownerId, query, ct);
        return Results.Ok(ownerProperties);
    }
    catch (Exception ex)
    {
        await ProblemDetailsMiddleware.WriteProblemDetails(http, StatusCodes.Status500InternalServerError, "Internal Server Error", "Failed to retrieve owner properties");
        return Results.Problem("Failed to retrieve owner properties");
    }
}).WithTags("Authentication");

app.MapGet("/auth/owner/properties/stats", [RequiresAuth] async (
    IPropertyService propertyService,
    IAuthService authService,
    HttpContext http,
    CancellationToken ct) =>
{
    try
    {
        // Get owner ID from JWT token
        var ownerId = http.User.FindFirst("ownerId")?.Value;
        if (string.IsNullOrEmpty(ownerId))
        {
            await ProblemDetailsMiddleware.WriteProblemDetails(http, StatusCodes.Status401Unauthorized, "Unauthorized", "Owner ID not found in token");
            return Results.Unauthorized();
        }

        // Get owner properties for stats
        var properties = await propertyService.GetPropertiesByOwnerAsync(ownerId, null, ct);

        var stats = new
        {
            totalProperties = properties.Total,
            activeProperties = properties.Items.Count(p => p.Status == "Active"),
            soldProperties = properties.Items.Count(p => p.Status == "Sold"),
            offMarketProperties = properties.Items.Count(p => p.Status == "OffMarket"),
            totalValue = properties.Items.Sum(p => p.Price),
            averagePrice = properties.Items.Any() ? properties.Items.Average(p => p.Price) : 0,
            propertiesByType = properties.Items.GroupBy(p => p.Status)
                .ToDictionary(g => g.Key, g => g.Count()),
            propertiesByCity = properties.Items.GroupBy(p => p.Address.Split(',').LastOrDefault()?.Trim() ?? "Unknown")
                .ToDictionary(g => g.Key, g => g.Count())
        };

        return Results.Ok(stats);
    }
    catch (Exception ex)
    {
        await ProblemDetailsMiddleware.WriteProblemDetails(http, StatusCodes.Status500InternalServerError, "Internal Server Error", "Failed to retrieve owner statistics");
        return Results.Problem("Failed to retrieve owner statistics");
    }
}).WithTags("Authentication");

// Protected endpoints (authentication required)
app.MapPost("/properties", [RequiresAuth] async (
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

app.MapPut("/properties/{id}", [RequiresAuth] async (
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

app.MapDelete("/properties/{id}", [RequiresAuth] async (
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

app.MapPatch("/properties/{id}/activate", [RequiresAuth] async (
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

app.MapPatch("/properties/{id}/deactivate", [RequiresAuth] async (
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

// Public media viewing endpoint (no authentication required)
app.MapGet("/properties/{id}/media", async (
    string id,
    IPropertyRepository repository,
    HttpContext http,
    CancellationToken ct) =>
{
    try
    {
        var property = await repository.GetByIdAsync(id, ct);
        if (property == null)
        {
            await ProblemDetailsMiddleware.WriteProblemDetails(http, StatusCodes.Status404NotFound, "Resource Not Found", $"Property {id} not found");
            return Results.NotFound();
        }

        var media = property.Media?.Where(m => m.Enabled).OrderBy(m => m.Index).ToList() ?? new List<Media>();
        var cover = property.Cover;

        return Results.Ok(new
        {
            cover = cover != null ? new
            {
                type = cover.Type.ToString(),
                url = cover.Url,
                poster = cover.Poster,
                index = cover.Index
            } : null,
            gallery = media.Select(m => new
            {
                id = m.Id,
                type = m.Type.ToString(),
                url = m.Url,
                poster = m.Poster,
                index = m.Index,
                enabled = m.Enabled,
                featured = m.Featured
            }).ToList()
        });
    }
    catch (Exception ex)
    {
        await ProblemDetailsMiddleware.WriteProblemDetails(http, StatusCodes.Status500InternalServerError, "Internal Server Error", $"Failed to retrieve media: {ex.Message}");
        return Results.Problem($"Failed to retrieve media: {ex.Message}");
    }
}).WithTags("Properties");

app.MapPatch("/properties/{id}/media", [RequiresAuth] async (
    string id,
    MediaPatchDto request,
    IMediaManagementService mediaService,
    HttpContext http,
    CancellationToken ct) =>
{
    try
    {
        var userId = http.User.FindFirst("ownerId")?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            await ProblemDetailsMiddleware.WriteProblemDetails(http, StatusCodes.Status401Unauthorized, "Unauthorized", "Owner ID not found in token");
            return Results.Unauthorized();
        }

        var updatedProperty = await mediaService.UpdatePropertyMediaAsync(id, request, userId, ct);
        return Results.Ok(updatedProperty);
    }
    catch (Exception ex)
    {
        await ProblemDetailsMiddleware.WriteProblemDetails(http, StatusCodes.Status500InternalServerError, "Internal Server Error", $"Failed to update media: {ex.Message}");
        return Results.Problem($"Failed to update media: {ex.Message}");
    }
}).WithTags("Properties");

// Public property timeline endpoint (no authentication required)
app.MapGet("/properties/{id}/traces", async (
    string id,
    IPropertyTraceService traceService,
    HttpContext http,
    CancellationToken ct) =>
{
    try
    {
        var traces = await traceService.GetPropertyTimelineAsync(id, ct);
        return Results.Ok(traces);
    }
    catch (Exception ex)
    {
        await ProblemDetailsMiddleware.WriteProblemDetails(http, StatusCodes.Status500InternalServerError, "Internal Server Error", $"Failed to retrieve property traces: {ex.Message}");
        return Results.Problem($"Failed to retrieve property traces: {ex.Message}");
    }
}).WithTags("Properties");

// Advanced Search endpoint
app.MapPost("/properties/search", async (
    AdvancedSearchRequest request,
    IAdvancedSearchService searchService,
    CancellationToken ct) =>
{
    try
    {
        var results = await searchService.SearchPropertiesAsync(request, ct);
        return Results.Ok(results);
    }
    catch (Exception ex)
    {
        return Results.Problem($"Search failed: {ex.Message}");
    }
}).WithTags("Properties");

// Public Property Statistics endpoint (no authentication required)
app.MapGet("/stats/properties", async (
    string? city,
    string? propertyType,
    IPropertyStatsService statsService,
    HttpContext http,
    CancellationToken ct) =>
{
    try
    {
        PropertyStatsDto stats;
        if (!string.IsNullOrEmpty(city))
        {
            stats = await statsService.GetStatsByCityAsync(city, ct);
        }
        else if (!string.IsNullOrEmpty(propertyType))
        {
            stats = await statsService.GetStatsByPropertyTypeAsync(propertyType, ct);
        }
        else
        {
            stats = await statsService.GetOverallStatsAsync(ct);
        }

        return Results.Ok(stats);
    }
    catch (Exception ex)
    {
        await ProblemDetailsMiddleware.WriteProblemDetails(http, StatusCodes.Status500InternalServerError, "Internal Server Error", $"Failed to retrieve statistics: {ex.Message}");
        return Results.Problem($"Failed to retrieve statistics: {ex.Message}");
    }
}).WithTags("Statistics");

// Webhooks endpoint for real-time updates
app.MapPost("/webhooks/property-updated", async (
    WebhookRequest request,
    IWebhookService webhookService,
    CancellationToken ct) =>
{
    try
    {
        var response = await webhookService.ProcessPropertyUpdateAsync(request, ct);
        return Results.Ok(response);
    }
    catch (Exception ex)
    {
        return Results.Problem($"Webhook processing failed: {ex.Message}");
    }
}).WithTags("Webhooks");

// Metrics endpoint for monitoring
app.MapGet("/metrics", async (
    CancellationToken ct) =>
{
    try
    {
        // Basic metrics for now - can be expanded with Prometheus format
        var metrics = new
        {
            timestamp = DateTime.UtcNow,
            uptime = Environment.TickCount64,
            memory = GC.GetTotalMemory(false),
            activeConnections = 0, // Placeholder
            requestsPerSecond = 0, // Placeholder
            errorRate = 0.0 // Placeholder
        };

        return Results.Ok(metrics);
    }
    catch (Exception ex)
    {
        return Results.Problem($"Failed to retrieve metrics: {ex.Message}");
    }
}).WithTags("Monitoring");

// Public owners endpoint (no authentication required)
app.MapGet("/owners", async (
    IOwnerRepository ownerRepository,
    CancellationToken ct) =>
{
    try
    {
        var owners = await ownerRepository.FindAsync(null, 1, 1000, ct);

        var publicOwners = owners.Select(o => new OwnerProfessionalDto
        {
            Id = o.Id,
            FullName = o.FullName,
            Email = o.Email,
            PhoneE164 = o.PhoneE164,
            PhotoUrl = o.PhotoUrl,
            Role = GetRoleDisplayName(o.Role),
            IsActive = o.IsActive,
            Title = o.Title,
            Bio = o.Bio ?? o.Description,
            Experience = o.ExperienceYears,
            PropertiesSold = o.PropertiesSold,
            Rating = o.Rating,
            Specialties = o.Specialties,
            Languages = o.Languages,
            Certifications = o.Certifications,
            Location = o.Location,
            Address = o.Address,
            Timezone = o.Timezone,
            Company = o.Company ?? "MILLION Luxury Real Estate",
            Department = o.Department,
            EmployeeId = o.EmployeeId,
            IsAvailable = o.IsAvailable,
            Schedule = o.Schedule,
            ResponseTime = o.ResponseTime,
            TotalSalesValue = o.TotalSalesValue,
            AveragePrice = o.AveragePrice,
            ClientSatisfaction = o.ClientSatisfaction,
            LinkedInUrl = o.LinkedInUrl,
            InstagramUrl = o.InstagramUrl,
            FacebookUrl = o.FacebookUrl,
            CreatedAt = o.CreatedAt,
            UpdatedAt = o.UpdatedAt,
            LastActive = o.LastActive
        }).ToList();

        return Results.Ok(publicOwners);
    }
    catch (Exception ex)
    {
        return Results.Problem($"Failed to retrieve owners: {ex.Message}");
    }
}).WithTags("Owners");

// Owner profile by slug endpoint (no authentication required)
app.MapGet("/owners/profiles/{slug}", async (
    string slug,
    IOwnerProfileService ownerProfileService,
    CancellationToken ct) =>
{
    try
    {
        var profile = await ownerProfileService.GetProfileBySlugAsync(slug, ct);
        if (profile == null)
        {
            return Results.NotFound(new { message = "Owner profile not found" });
        }

        return Results.Ok(profile);
    }
    catch (Exception ex)
    {
        return Results.Problem($"Failed to retrieve owner profile: {ex.Message}");
    }
}).WithTags("Owners");

// Owner profile endpoint (requires authentication)
app.MapGet("/owners/profile", [RequiresAuth] async (
    IAuthService authService,
    HttpContext http,
    CancellationToken ct) =>
{
    try
    {
        // Get owner ID from JWT token (already validated by middleware)
        var ownerId = http.User.FindFirst("ownerId")?.Value;
        if (string.IsNullOrEmpty(ownerId))
        {
            await ProblemDetailsMiddleware.WriteProblemDetails(http, StatusCodes.Status401Unauthorized, "Unauthorized", "Owner ID not found in token");
            return Results.Unauthorized();
        }

        var owner = await authService.GetOwnerByIdAsync(ownerId, ct);
        if (owner == null)
        {
            await ProblemDetailsMiddleware.WriteProblemDetails(http, StatusCodes.Status404NotFound, "Owner Not Found", "Owner not found");
            return Results.NotFound();
        }

        // Return owner profile without sensitive information
        return Results.Ok(new
        {
            id = owner.Id,
            fullName = owner.FullName,
            email = owner.Email,
            phoneE164 = owner.PhoneE164,
            photoUrl = owner.PhotoUrl,
            role = owner.Role,
            isActive = owner.IsActive,
            createdAt = owner.CreatedAt,
            updatedAt = owner.UpdatedAt
        });
    }
    catch (Exception ex)
    {
        await ProblemDetailsMiddleware.WriteProblemDetails(http, StatusCodes.Status500InternalServerError, "Internal Server Error", "Failed to retrieve owner profile");
        return Results.Problem("Failed to retrieve owner profile");
    }
}).WithTags("Owners");

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
public partial class Program
{
    // Helper method to convert OwnerRole enum to display names
    private static string GetRoleDisplayName(OwnerRole role) => role switch
    {
        OwnerRole.Owner => "Owner",
        OwnerRole.Admin => "Admin",
        OwnerRole.CEO => "CEO & Founder",
        OwnerRole.HeadOfSales => "Head of Sales",
        OwnerRole.LeadDesigner => "Lead Designer",
        OwnerRole.InvestmentAdvisor => "Investment Advisor",
        OwnerRole.SeniorAgent => "Senior Agent",
        OwnerRole.PropertyManager => "Property Manager",
        OwnerRole.CommercialSpecialist => "Commercial Specialist",
        _ => role.ToString()
    };
}
