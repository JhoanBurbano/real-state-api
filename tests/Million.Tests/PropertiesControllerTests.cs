using Million.Application.DTOs;
using Million.Application.Interfaces;
using Million.Application.Services;
using Million.Domain.Entities;
using Million.Infrastructure.Repositories;
using Million.Application.Common;
using NSubstitute;

namespace Million.Tests;

public class PropertiesControllerTests
{
    private IPropertyService _mockService;
    private IPropertyRepository _mockRepository;

    [SetUp]
    public void Setup()
    {
        _mockService = Substitute.For<IPropertyService>();
        _mockRepository = Substitute.For<IPropertyRepository>();
    }

    [Test]
    public async Task CreateProperty_WithCoverOnly_Returns201()
    {
        // Arrange
        var request = new CreatePropertyRequest
        {
            OwnerId = "owner123",
            Name = "Luxury Villa",
            Description = "Beautiful luxury villa with ocean view and modern amenities",
            Address = "123 Ocean Drive, Miami Beach, FL",
            Price = 2500000,
            CoverImage = "https://0daikfjw6ec1yprw.public.blob.vercel-storage.com/properties/prop123/cover.jpg"
        };

        var createdProperty = new PropertyDto
        {
            Id = "prop123",
            OwnerId = "owner123",
            Name = "Luxury Villa",
            Description = "Beautiful luxury villa with ocean view and modern amenities",
            Address = "123 Ocean Drive, Miami Beach, FL",
            Price = 2500000,
            CoverImage = "https://0daikfjw6ec1yprw.public.blob.vercel-storage.com/properties/prop123/cover.jpg",
            Images = Array.Empty<string>()
        };

        _mockService.CreatePropertyAsync(request, Arg.Any<CancellationToken>())
            .Returns(createdProperty);

        // Act
        var result = await _mockService.CreatePropertyAsync(request, CancellationToken.None);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo("prop123"));
        Assert.That(result.Name, Is.EqualTo("Luxury Villa"));
        Assert.That(result.Price, Is.EqualTo(2500000));
    }

    [Test]
    public async Task CreateProperty_WithGallery_Returns201()
    {
        // Arrange
        var request = new CreatePropertyRequest
        {
            OwnerId = "owner123",
            Name = "Luxury Villa",
            Description = "Beautiful luxury villa with gallery and modern amenities",
            Address = "123 Ocean Drive, Miami Beach, FL",
            Price = 2500000,
            CoverImage = "https://0daikfjw6ec1yprw.public.blob.vercel-storage.com/properties/prop123/cover.jpg",
            Images = new[]
            {
                "https://0daikfjw6ec1yprw.public.blob.vercel-storage.com/properties/prop123/1.jpg",
                "https://0daikfjw6ec1yprw.public.blob.vercel-storage.com/properties/prop123/2.jpg",
                "https://0daikfjw6ec1yprw.public.blob.vercel-storage.com/properties/prop123/3.jpg"
            }
        };

        var createdProperty = new PropertyDto
        {
            Id = "prop123",
            OwnerId = "owner123",
            Name = "Luxury Villa",
            Description = "Beautiful luxury villa with gallery and modern amenities",
            Address = "123 Ocean Drive, Miami Beach, FL",
            Price = 2500000,
            CoverImage = "https://0daikfjw6ec1yprw.public.blob.vercel-storage.com/properties/prop123/cover.jpg",
            Images = new[]
            {
                "https://0daikfjw6ec1yprw.public.blob.vercel-storage.com/properties/prop123/1.jpg",
                "https://0daikfjw6ec1yprw.public.blob.vercel-storage.com/properties/prop123/2.jpg",
                "https://0daikfjw6ec1yprw.public.blob.vercel-storage.com/properties/prop123/3.jpg"
            }
        };

        _mockService.CreatePropertyAsync(request, Arg.Any<CancellationToken>())
            .Returns(createdProperty);

        // Act
        var result = await _mockService.CreatePropertyAsync(request, CancellationToken.None);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Images, Has.Length.EqualTo(3));
        Assert.That(result.Images[0], Is.EqualTo("https://0daikfjw6ec1yprw.public.blob.vercel-storage.com/properties/prop123/1.jpg"));
    }

    [Test]
    public async Task GetPropertyById_WhenFound_Returns200()
    {
        // Arrange
        var propertyId = "prop123";
        var property = new PropertyDto
        {
            Id = "prop123",
            OwnerId = "owner123",
            Name = "Luxury Villa",
            Description = "Beautiful luxury villa with ocean view and modern amenities",
            Address = "123 Ocean Drive, Miami Beach, FL",
            Price = 2500000,
            CoverImage = "https://0daikfjw6ec1yprw.public.blob.vercel-storage.com/properties/prop123/cover.jpg",
            Images = new[]
            {
                "https://0daikfjw6ec1yprw.public.blob.vercel-storage.com/properties/prop123/1.jpg",
                "https://0daikfjw6ec1yprw.public.blob.vercel-storage.com/properties/prop123/2.jpg"
            }
        };

        _mockService.GetByIdAsync(propertyId, Arg.Any<CancellationToken>())
            .Returns(property);

        // Act
        var result = await _mockService.GetByIdAsync(propertyId, CancellationToken.None);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo(propertyId));
        Assert.That(result.Images, Has.Length.EqualTo(2));
    }

    [Test]
    public async Task GetPropertyById_WhenNotFound_ReturnsNull()
    {
        // Arrange
        var propertyId = "nonexistent";

        _mockService.GetByIdAsync(propertyId, Arg.Any<CancellationToken>())
            .Returns((PropertyDto?)null);

        // Act
        var result = await _mockService.GetByIdAsync(propertyId, CancellationToken.None);

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task GetProperties_WithValidQuery_ReturnsPagedResult()
    {
        // Arrange
        var query = new PropertyListQuery
        {
            Page = 1,
            PageSize = 10,
            MinPrice = 1000000,
            MaxPrice = 5000000
        };

        var properties = new List<PropertyListDto>
        {
            new()
            {
                Id = "prop1",
                OwnerId = "owner1",
                Name = "Villa 1",
                Address = "Address 1",
                Price = 2000000,
                CoverUrl = "https://0daikfjw6ec1yprw.public.blob.vercel-storage.com/properties/prop1/prop-prop1_photo-01.jpg",
                HasMoreMedia = false,
                TotalImages = 0,
                TotalVideos = 0
            },
            new()
            {
                Id = "prop2",
                OwnerId = "owner2",
                Name = "Villa 2",
                Address = "Address 2",
                Price = 3000000,
                CoverUrl = "https://0daikfjw6ec1yprw.public.blob.vercel-storage.com/properties/prop2/prop-prop2_photo-01.jpg",
                HasMoreMedia = true,
                TotalImages = 1,
                TotalVideos = 0
            }
        };

        var pagedResult = new PagedResult<PropertyListDto>
        {
            Items = properties,
            Total = 2,
            Page = 1,
            PageSize = 10
        };

        _mockService.GetPropertiesAsync(query, Arg.Any<CancellationToken>())
            .Returns(pagedResult);

        // Act
        var result = await _mockService.GetPropertiesAsync(query, CancellationToken.None);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Items, Has.Count.EqualTo(2));
        Assert.That(result.Total, Is.EqualTo(2));
        Assert.That(result.Items[0].TotalImages, Is.EqualTo(0));
        Assert.That(result.Items[1].TotalImages, Is.EqualTo(1));
    }
}
