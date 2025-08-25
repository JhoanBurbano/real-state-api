using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Million.Web;
using Million.Infrastructure.Persistence;
using Million.Infrastructure.Migrations;
using Million.Application.Services;
using Million.Infrastructure.Services;
using Million.Infrastructure.Repositories;
using Million.Application.Interfaces;
using Million.Application.Validation;
using Million.Web.Middlewares;
using FluentValidation;
using FluentValidation.AspNetCore;
using Testcontainers.MongoDb;
using FluentAssertions;
using NUnit.Framework;
using Newtonsoft.Json;
using System.Net.Http.Json;

namespace Million.E2E.Tests;

[TestFixture]
public abstract class TestBase : IDisposable
{
    protected WebApplicationFactory<Program> Factory { get; private set; } = null!;
    protected HttpClient Client { get; private set; } = null!;
    protected MongoDbContainer MongoContainer { get; private set; } = null!;
    protected MongoContext MongoContext { get; private set; } = null!;
    protected IServiceScope ServiceScope { get; private set; } = null!;

    [OneTimeSetUp]
    public async Task OneTimeSetUpAsync()
    {
        // Start MongoDB container
        MongoContainer = new MongoDbBuilder()
            .WithImage("mongo:7.0")
            .WithUsername("test")
            .WithPassword("test123")
            .Build();

        await MongoContainer.StartAsync();

        // Create factory with test configuration
        Factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration((context, config) =>
                {
                    config.AddInMemoryCollection(new Dictionary<string, string?>
                    {
                        ["MONGO_URI"] = MongoContainer.GetConnectionString(),
                        ["MONGO_DB"] = "million",
                        ["ASPNETCORE_ENVIRONMENT"] = "Testing",
                        ["DOTNET_ENVIRONMENT"] = "Testing",
                        ["SkipProblemDetailsMiddleware"] = "true",
                        ["SkipJwtAuthMiddleware"] = "true",
                        ["LOG_LEVEL"] = "Warning",
                        ["RATE_LIMIT_PERMINUTE"] = "1000",
                        ["RATE_LIMIT_BURST"] = "2000",
                        ["RATE_LIMIT_ENABLE_BURST"] = "true",
                        ["AUTH_JWT_ISSUER"] = "https://test.million.com",
                        ["AUTH_JWT_AUDIENCE"] = "https://test.million.com",
                        ["AUTH_ACCESS_TTL_MIN"] = "60",
                        ["AUTH_REFRESH_TTL_DAYS"] = "30",
                        ["AUTH_LOCKOUT_ATTEMPTS"] = "10",
                        ["AUTH_LOCKOUT_WINDOW_MIN"] = "30",
                        ["FEATURED_MEDIA_LIMIT"] = "12",
                        ["MEDIA_LIBRARY_LIMIT"] = "60",
                        ["MAX_UPLOAD_MB"] = "25",
                        ["ENABLE_VIDEO"] = "false"
                    });
                });

                builder.ConfigureServices(services =>
                {
                    // Remove existing MongoDB configuration
                    var descriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(MongoContext));
                    if (descriptor != null)
                    {
                        services.Remove(descriptor);
                    }

                    // Remove ProblemDetailsMiddleware to avoid conflicts in testing
                    var problemDetailsDescriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(ProblemDetailsMiddleware));
                    if (problemDetailsDescriptor != null)
                    {
                        services.Remove(problemDetailsDescriptor);
                    }

                    // Add test MongoDB configuration
                    services.AddSingleton<MongoContext>(provider =>
                    {
                        var options = new MongoOptions
                        {
                            Uri = MongoContainer.GetConnectionString(),
                            Database = "million",
                            RootUsername = "admin",
                            RootPassword = "EmpanadasConAji123"
                        };
                        var optionsWrapper = Options.Create(options);
                        return new MongoContext(optionsWrapper);
                    });

                    // Configure logging for tests
                    services.AddLogging(logging =>
                    {
                        logging.ClearProviders();
                        logging.AddConsole();
                        logging.SetMinimumLevel(LogLevel.Warning);
                    });

                    // Add all required services for testing (as singletons to avoid DI issues)
                    services.AddSingleton<IPropertyRepository, PropertyRepository>();
                    services.AddSingleton<IPropertyService, PropertyService>();
                    services.AddSingleton<IOwnerRepository, OwnerRepository>();
                    services.AddSingleton<IOwnerSessionRepository, OwnerSessionRepository>();
                    services.AddSingleton<IAuthService, AuthService>();
                    services.AddSingleton<IJwtService, JwtService>();
                    services.AddSingleton<IPasswordService, PasswordService>();
                    services.AddSingleton<IExplainService, ExplainService>();

                    // Add validation
                    services.AddFluentValidationAutoValidation();
                    services.AddValidatorsFromAssemblyContaining<PropertyListQueryValidator>();

                    // Add memory cache for rate limiting
                    services.AddMemoryCache();
                    services.AddSingleton<SimpleRateLimiter>();
                });
            });

        Client = Factory.CreateClient();
        ServiceScope = Factory.Services.CreateScope();
        MongoContext = ServiceScope.ServiceProvider.GetRequiredService<MongoContext>();

        // Initialize test database
        await InitializeTestDatabaseAsync();

        // Debug: Log the environment configuration
        Console.WriteLine($"=== TESTBASE ENVIRONMENT DEBUG ===");
        Console.WriteLine($"ASPNETCORE_ENVIRONMENT: {Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}");
        Console.WriteLine($"DOTNET_ENVIRONMENT: {Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")}");
        Console.WriteLine($"Current Environment: {Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "NOT SET"}");
        Console.WriteLine($"=====================================");
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDownAsync()
    {
        Client?.Dispose();
        Factory?.Dispose();
        ServiceScope?.Dispose();

        if (MongoContainer != null)
        {
            await MongoContainer.DisposeAsync();
        }
    }

    protected virtual async Task InitializeTestDatabaseAsync()
    {
        // Create indexes
        await EnsureIndexes.RunAsync(MongoContext, CancellationToken.None);

        // Seed test data
        var passwordService = ServiceScope.ServiceProvider.GetRequiredService<IPasswordService>();
        await SeedData.SeedAsync(MongoContext, passwordService, CancellationToken.None);
    }

    protected async Task<T?> DeserializeResponseAsync<T>(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<T>(content);
    }

    protected async Task<HttpResponseMessage> AuthenticateAsOwnerAsync(string email = "carlos.rodriguez@million.com", string password = "Password123!")
    {
        var loginRequest = new
        {
            Email = email,
            Password = password
        };

        var response = await Client.PostAsJsonAsync("/auth/owner/login", loginRequest);
        response.EnsureSuccessStatusCode();
        return response;
    }

    protected async Task<HttpResponseMessage> AuthenticateAsAdminAsync(string email = "admin@million.com", string password = "Admin123!")
    {
        var loginRequest = new
        {
            Email = email,
            Password = password
        };

        var response = await Client.PostAsJsonAsync("/auth/owner/login", loginRequest);
        response.EnsureSuccessStatusCode();
        return response;
    }

    protected async Task<string> GetAccessTokenAsync(HttpResponseMessage authResponse)
    {
        var loginResponse = await DeserializeResponseAsync<dynamic>(authResponse);
        return loginResponse?.accessToken?.ToString() ?? string.Empty;
    }

    protected HttpClient CreateAuthenticatedClient(string accessToken)
    {
        var client = Factory.CreateClient();
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");
        return client;
    }

    protected async Task<HttpClient> CreateOwnerClientAsync(string email = "carlos.rodriguez@million.com", string password = "Password123!")
    {
        var authResponse = await AuthenticateAsOwnerAsync(email, password);
        var accessToken = await GetAccessTokenAsync(authResponse);
        return CreateAuthenticatedClient(accessToken);
    }

    protected async Task<HttpClient> CreateAdminClientAsync(string email = "admin@million.com", string password = "Admin123!")
    {
        var authResponse = await AuthenticateAsAdminAsync(email, password);
        var accessToken = await GetAccessTokenAsync(authResponse);
        return CreateAuthenticatedClient(accessToken);
    }

    public void Dispose()
    {
        ServiceScope?.Dispose();
        Client?.Dispose();
        Factory?.Dispose();
        GC.SuppressFinalize(this);
    }
}
