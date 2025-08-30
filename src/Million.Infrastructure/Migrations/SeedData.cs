using MongoDB.Driver;
using Million.Application.Services;
using Million.Domain.Entities;
using Million.Infrastructure.Persistence;

namespace Million.Infrastructure.Migrations;

public static class SeedData
{
    public static async Task SeedAsync(MongoContext context, IPasswordService passwordService, CancellationToken cancellationToken = default)
    {
        await SeedOwnersAsync(context, passwordService, cancellationToken);
        await SeedPropertiesAsync(context, cancellationToken);
    }

    private static async Task SeedOwnersAsync(MongoContext context, IPasswordService passwordService, CancellationToken cancellationToken)
    {
        var ownersCollection = context.Database.GetCollection<OwnerDocument>("owners");

        // Check if admin already exists
        var existingAdmin = await ownersCollection.Find(x => x.Role == OwnerRole.Admin).FirstOrDefaultAsync(cancellationToken);
        if (existingAdmin == null)
        {
            var adminOwner = new Owner
            {
                Id = Guid.NewGuid().ToString(),
                FullName = "System Administrator",
                Email = "admin@million.com",
                Role = OwnerRole.Admin,
                PasswordHash = passwordService.HashPassword("Admin123!"),
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var adminDocument = OwnerDocument.FromEntity(adminOwner);
            await ownersCollection.InsertOneAsync(adminDocument, cancellationToken: cancellationToken);
        }

        // Check if sample owner exists
        var existingOwner = await ownersCollection.Find(x => x.Role == OwnerRole.Owner).FirstOrDefaultAsync(cancellationToken);
        if (existingOwner == null)
        {
            var sampleOwner = new Owner
            {
                Id = Guid.NewGuid().ToString(),
                FullName = "John Doe",
                Email = "john.doe@example.com",
                Role = OwnerRole.Owner,
                PhoneE164 = "+1234567890",
                PhotoUrl = "https://example.com/photos/john.jpg",
                PasswordHash = passwordService.HashPassword("Password123!"),
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var ownerDocument = OwnerDocument.FromEntity(sampleOwner);
            await ownersCollection.InsertOneAsync(ownerDocument, cancellationToken: cancellationToken);
        }
    }

    private static async Task SeedPropertiesAsync(MongoContext context, CancellationToken cancellationToken)
    {
        var propertiesCollection = context.Database.GetCollection<PropertyDocument>("properties");

        // Check if properties already exist
        var existingCount = await propertiesCollection.CountDocumentsAsync(FilterDefinition<PropertyDocument>.Empty, cancellationToken: cancellationToken);
        if (existingCount > 0) return;

        // Get the sample owner
        var ownersCollection = context.Database.GetCollection<OwnerDocument>("owners");
        var sampleOwner = await ownersCollection.Find(x => x.Role == OwnerRole.Owner).FirstOrDefaultAsync(cancellationToken);
        if (sampleOwner == null) return;

        var sampleProperties = new[]
        {
            new Property
            {
                Id = Guid.NewGuid().ToString(),
                OwnerId = sampleOwner.Id,
                Name = "Luxury Villa with Ocean View",
                Address = "123 Ocean Drive, Malibu, CA",
                Price = 2500000,
                CodeInternal = "MLB001",
                Year = 2020,
                Status = PropertyStatus.Active,
                Cover = Cover.CreateImage("https://0daikfjw6ec1yprw.public.blob.vercel-storage.com/properties/mlb001/prop-mlb001_photo-01.jpg"),
                Media = new List<Media>
                {
                    new Media { Id = Guid.NewGuid().ToString(), Type = MediaType.Image, Url = "https://0daikfjw6ec1yprw.public.blob.vercel-storage.com/properties/mlb001/1.jpg", Index = 1, Enabled = true, Featured = true },
                    new Media { Id = Guid.NewGuid().ToString(), Type = MediaType.Image, Url = "https://0daikfjw6ec1yprw.public.blob.vercel-storage.com/properties/mlb001/2.jpg", Index = 2, Enabled = true, Featured = true },
                    new Media { Id = Guid.NewGuid().ToString(), Type = MediaType.Image, Url = "https://0daikfjw6ec1yprw.public.blob.vercel-storage.com/properties/mlb001/3.jpg", Index = 3, Enabled = true, Featured = true },
                    new Media { Id = Guid.NewGuid().ToString(), Type = MediaType.Image, Url = "https://0daikfjw6ec1yprw.public.blob.vercel-storage.com/properties/mlb001/4.jpg", Index = 4, Enabled = true, Featured = false },
                    new Media { Id = Guid.NewGuid().ToString(), Type = MediaType.Image, Url = "https://0daikfjw6ec1yprw.public.blob.vercel-storage.com/properties/mlb001/5.jpg", Index = 5, Enabled = true, Featured = false }
                },
                Description = "Stunning luxury villa with panoramic ocean views",
                City = "Malibu",
                Neighborhood = "Point Dume",
                PropertyType = "Villa",
                Size = 450,
                Bedrooms = 4,
                Bathrooms = 5,
                HasPool = true,
                HasGarden = true,
                HasParking = true,
                IsFurnished = true,
                AvailableFrom = DateTime.UtcNow.AddDays(30),
                AvailableTo = DateTime.UtcNow.AddDays(365),
                Traces = new List<PropertyTrace>
                {
                    PropertyTrace.Create(
                        Guid.NewGuid().ToString(),
                        TraceAction.Sold,
                        "2200000",
                        "2350000",
                        sampleOwner.Id,
                        "Previous sale transaction"
                    )
                },
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsActive = true
            },
            new Property
            {
                Id = Guid.NewGuid().ToString(),
                OwnerId = sampleOwner.Id,
                Name = "Modern Downtown Penthouse",
                Address = "456 Downtown Ave, Los Angeles, CA",
                Price = 1800000,
                CodeInternal = "LAX001",
                Year = 2022,
                Status = PropertyStatus.Active,
                Cover = Cover.CreateImage("https://0daikfjw6ec1yprw.public.blob.vercel-storage.com/properties/lax001/prop-lax001_photo-01.jpg"),
                Media = new List<Media>
                {
                    new Media { Id = Guid.NewGuid().ToString(), Type = MediaType.Image, Url = "https://0daikfjw6ec1yprw.public.blob.vercel-storage.com/properties/lax001/1.jpg", Index = 1, Enabled = true, Featured = true },
                    new Media { Id = Guid.NewGuid().ToString(), Type = MediaType.Image, Url = "https://0daikfjw6ec1yprw.public.blob.vercel-storage.com/properties/lax001/2.jpg", Index = 2, Enabled = true, Featured = true },
                    new Media { Id = Guid.NewGuid().ToString(), Type = MediaType.Image, Url = "https://0daikfjw6ec1yprw.public.blob.vercel-storage.com/properties/lax001/3.jpg", Index = 3, Enabled = true, Featured = false }
                },
                Description = "Contemporary penthouse in the heart of downtown",
                City = "Los Angeles",
                Neighborhood = "Downtown",
                PropertyType = "Penthouse",
                Size = 280,
                Bedrooms = 3,
                Bathrooms = 3,
                HasPool = false,
                HasGarden = false,
                HasParking = true,
                IsFurnished = false,
                AvailableFrom = DateTime.UtcNow.AddDays(15),
                AvailableTo = DateTime.UtcNow.AddDays(180),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsActive = true
            }
        };

        foreach (var property in sampleProperties)
        {
            var document = PropertyDocument.FromEntity(property);
            await propertiesCollection.InsertOneAsync(document, cancellationToken: cancellationToken);
        }
    }
}
