using NUnit.Framework;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Million.Infrastructure.Persistence;
using Million.Infrastructure.Migrations;
using Million.Application.Services;
using Million.Infrastructure.Services;

namespace Million.E2E.Tests;

[SetUpFixture]
public class GlobalSetup
{
    public static IHost TestHost { get; private set; } = null!;
    public static IServiceScope TestScope { get; private set; } = null!;
    public static MongoContext TestMongoContext { get; private set; } = null!;

    [OneTimeSetUp]
    public async Task GlobalOneTimeSetUp()
    {
        // Configure test environment
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ASPNETCORE_ENVIRONMENT"] = "Testing",
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
            })
            .Build();

        // Create test host
        TestHost = Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration((context, config) =>
            {
                config.AddConfiguration(configuration);
            })
            .ConfigureServices(services =>
            {
                // Add test services
                services.AddSingleton<IConfiguration>(configuration);

                // Configure test MongoDB context
                services.AddSingleton<MongoContext>(provider =>
                {
                    var options = new MongoOptions
                    {
                        Uri = "mongodb+srv://jsburbano:EmpanadasConAji123@pruebastecnicas.sm4lf1d.mongodb.net/?retryWrites=true&w=majority&appName=pruebastecnicas",
                        Database = "million",
                        RootUsername = "admin",
                        RootPassword = "EmpanadasConAji123"
                    };
                    var optionsWrapper = Options.Create(options);
                    return new MongoContext(optionsWrapper);
                });

                // Add other required services
                services.AddMemoryCache();
                services.AddHttpClient();

                // Add application services needed for seeding
                services.AddScoped<IPasswordService, PasswordService>();
            })
            .Build();

        await TestHost.StartAsync();

        TestScope = TestHost.Services.CreateScope();
        TestMongoContext = TestScope.ServiceProvider.GetRequiredService<MongoContext>();

        // Initialize test database
        await InitializeTestDatabaseAsync();
    }

    [OneTimeTearDown]
    public async Task GlobalOneTimeTearDown()
    {
        TestScope?.Dispose();
        await TestHost?.StopAsync();
        TestHost?.Dispose();
    }

    private static async Task InitializeTestDatabaseAsync()
    {
        try
        {
            // Create indexes
            await EnsureIndexes.RunAsync(TestMongoContext, CancellationToken.None);

            // Seed test data
            var passwordService = TestScope.ServiceProvider.GetRequiredService<IPasswordService>();
            await SeedData.SeedAsync(TestMongoContext, passwordService, CancellationToken.None);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error initializing test database: {ex.Message}");
            throw;
        }
    }
}
