using NUnit.Framework;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Million.Infrastructure.Persistence;
using Million.Infrastructure.Migrations;
using Million.Application.Services;
using Million.Infrastructure.Services;
using MongoDB.Driver;
using MongoDB.Bson;
using Million.Domain.Entities;

namespace Million.E2E.Tests;

[SetUpFixture]
public class GlobalSetup
{
    private static MongoClient? _client;
    private static IMongoDatabase? _database;
    private static string? _connectionString;

    [OneTimeSetUp]
    public async Task GlobalSetupAsync()
    {
        // Load configuration from environment variables or appsettings
        var configuration = new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .AddJsonFile("appsettings.test.json", optional: true)
            .Build();

        // Use environment variable for MongoDB URI - NEVER hardcode credentials
        _connectionString = configuration.GetConnectionString("MongoDB")
            ?? Environment.GetEnvironmentVariable("MONGODB_URI")
            ?? "mongodb://localhost:27017";

        Console.WriteLine($"Connecting to MongoDB: {_connectionString.Replace(_connectionString.Split('@')[0], "***")}");

        try
        {
            _client = new MongoClient(_connectionString);
            _database = _client.GetDatabase("million_test");

            // Test connection
            await _database.RunCommandAsync((Command<BsonDocument>)"{ping:1}");
            Console.WriteLine("✅ MongoDB connection successful");

            // Clean up test database
            await CleanupTestDataAsync();
            Console.WriteLine("✅ Test database cleaned up");

            // Seed test data
            await SeedTestDataAsync();
            Console.WriteLine("✅ Test data seeded successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ MongoDB connection failed: {ex.Message}");
            throw;
        }
    }

    [OneTimeTearDown]
    public async Task GlobalTearDownAsync()
    {
        try
        {
            if (_database != null)
            {
                // Clean up test database
                await CleanupTestDataAsync();
                Console.WriteLine("✅ Test database cleaned up");
            }

            _client?.Dispose();
            Console.WriteLine("✅ MongoDB connection closed");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error during cleanup: {ex.Message}");
        }
    }

    private static async Task CleanupTestDataAsync()
    {
        if (_database == null) return;

        var collections = await _database.ListCollectionNamesAsync();
        var collectionNames = await collections.ToListAsync();

        foreach (var collectionName in collectionNames)
        {
            await _database.DropCollectionAsync(collectionName);
        }
    }

    private static async Task SeedTestDataAsync()
    {
        if (_database == null) return;

        // Seed test owners
        var ownersCollection = _database.GetCollection<OwnerDocument>("owners");
        var testOwners = new List<OwnerDocument>
        {
            new()
            {
                Id = "test-owner-001",
                FullName = "Test Owner 1",
                Email = "test1@example.com",
                PhoneE164 = "+1234567890",
                Role = Domain.Entities.OwnerRole.Owner,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = "test-owner-002",
                FullName = "Test Owner 2",
                Email = "test2@example.com",
                PhoneE164 = "+1234567891",
                Role = Domain.Entities.OwnerRole.Admin,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };

        await ownersCollection.InsertManyAsync(testOwners);

        // Seed test properties
        var propertiesCollection = _database.GetCollection<PropertyDocument>("properties");
        var testProperties = new List<PropertyDocument>
        {
            new()
            {
                Id = "test-prop-001",
                Name = "Test Property 1",
                Description = "Test property description",
                Address = "123 Test St, Test City, FL",
                Price = 1000000,
                Year = 2020,
                Status = Domain.Entities.PropertyStatus.Active,
                OwnerId = "test-owner-001",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = "test-prop-002",
                Name = "Test Property 2",
                Description = "Another test property",
                Address = "456 Test Ave, Test City, FL",
                Price = 2000000,
                Year = 2021,
                Status = Domain.Entities.PropertyStatus.Active,
                OwnerId = "test-owner-002",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };

        await propertiesCollection.InsertManyAsync(testProperties);
    }

    public static MongoClient? GetMongoClient() => _client;
    public static IMongoDatabase? GetTestDatabase() => _database;
}
