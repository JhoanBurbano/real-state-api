using FluentValidation.TestHelper;
using Million.Application.DTOs;
using Million.Application.Validation;

namespace Million.Tests;

public class PropertyCreateValidatorTests
{
    private readonly CreatePropertyRequestValidator _validator;

    public PropertyCreateValidatorTests()
    {
        _validator = new CreatePropertyRequestValidator();
    }

    [Test]
    public void Should_accept_valid_property_with_description()
    {
        // Arrange
        var request = new CreatePropertyRequest
        {
            OwnerId = "owner123",
            Name = "Luxury Villa",
            Description = "Beautiful luxury villa with ocean view, modern amenities, and spacious rooms. Perfect for families seeking comfort and elegance.",
            Address = "123 Ocean Drive",
            City = "Miami Beach",
            PropertyType = "Villa",
            Price = 2500000,
            CodeInternal = "LV001",
            Year = 2020,
            Size = 500,
            Bedrooms = 4,
            Bathrooms = 3,
            CoverImage = "https://0daikfjw6ec1yprw.public.blob.vercel-storage.com/properties/prop123/cover.jpg"
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Test]
    public void Should_reject_property_with_description_too_long()
    {
        // Arrange
        var request = new CreatePropertyRequest
        {
            OwnerId = "owner123",
            Name = "Luxury Villa",
            Description = new string('A', 1001), // Exceeds 1000 character limit
            Address = "123 Ocean Drive",
            City = "Miami Beach",
            PropertyType = "Villa",
            Price = 2500000,
            CodeInternal = "LV001",
            Year = 2020,
            Size = 500,
            Bedrooms = 4,
            Bathrooms = 3,
            CoverImage = "https://0daikfjw6ec1yprw.public.blob.vercel-storage.com/properties/prop123/cover.jpg"
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Description);
    }

    [Test]
    public void Should_accept_valid_property_with_cover_only()
    {
        // Arrange
        var request = new CreatePropertyRequest
        {
            OwnerId = "owner123",
            Name = "Luxury Villa",
            Description = "Luxury villa with modern amenities",
            Address = "123 Ocean Drive",
            City = "Miami Beach",
            PropertyType = "Villa",
            Price = 2500000,
            CodeInternal = "LV001",
            Year = 2020,
            Size = 500,
            Bedrooms = 4,
            Bathrooms = 3,
            CoverImage = "https://0daikfjw6ec1yprw.public.blob.vercel-storage.com/properties/prop123/cover.jpg"
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Test]
    public void Should_accept_valid_property_with_gallery()
    {
        // Arrange
        var request = new CreatePropertyRequest
        {
            OwnerId = "owner123",
            Name = "Luxury Villa",
            Description = "Luxury villa with gallery and modern amenities",
            Address = "123 Ocean Drive",
            City = "Miami Beach",
            PropertyType = "Villa",
            Price = 2500000,
            CodeInternal = "LV001",
            Year = 2020,
            Size = 500,
            Bedrooms = 4,
            Bathrooms = 3,
            CoverImage = "https://0daikfjw6ec1yprw.public.blob.vercel-storage.com/properties/prop123/cover.jpg",
            Images = new[]
            {
                "https://0daikfjw6ec1yprw.public.blob.vercel-storage.com/properties/prop123/1.jpg",
                "https://0daikfjw6ec1yprw.public.blob.vercel-storage.com/properties/prop123/2.jpg",
                "https://0daikfjw6ec1yprw.public.blob.vercel-storage.com/properties/prop123/3.jpg"
            }
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Test]
    public void Should_accept_valid_property_with_maximum_gallery()
    {
        // Arrange
        var request = new CreatePropertyRequest
        {
            OwnerId = "owner123",
            Name = "Luxury Villa",
            Description = "Luxury villa with maximum gallery and modern amenities",
            Address = "123 Ocean Drive",
            City = "Miami Beach",
            PropertyType = "Villa",
            Price = 2500000,
            CodeInternal = "LV001",
            Year = 2020,
            Size = 500,
            Bedrooms = 4,
            Bathrooms = 3,
            CoverImage = "https://0daikfjw6ec1yprw.public.blob.vercel-storage.com/properties/prop123/cover.jpg",
            Images = Enumerable.Range(1, 12)
                .Select(i => $"https://0daikfjw6ec1yprw.public.blob.vercel-storage.com/properties/prop123/{i}.jpg")
                .ToArray()
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Test]
    public void Should_reject_missing_cover_image()
    {
        // Arrange
        var request = new CreatePropertyRequest
        {
            OwnerId = "owner123",
            Name = "Luxury Villa",
            Description = "Luxury villa with modern amenities",
            Address = "123 Ocean Drive",
            City = "Miami Beach",
            PropertyType = "Villa",
            Price = 2500000,
            CodeInternal = "LV001",
            Year = 2020,
            Size = 500,
            Bedrooms = 4,
            Bathrooms = 3,
            CoverImage = ""
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        // With new validation logic, this should fail because no cover system is provided
        result.ShouldHaveValidationErrorFor(x => x);
    }

    [Test]
    public void Should_reject_gallery_exceeding_maximum()
    {
        // Arrange
        var request = new CreatePropertyRequest
        {
            OwnerId = "owner123",
            Name = "Luxury Villa",
            Description = "Luxury villa with gallery and modern amenities",
            Address = "123 Ocean Drive",
            City = "Miami Beach",
            PropertyType = "Villa",
            Price = 2500000,
            CodeInternal = "LV001",
            Year = 2020,
            Size = 500,
            Bedrooms = 4,
            Bathrooms = 3,
            CoverImage = "https://0daikfjw6ec1yprw.public.blob.vercel-storage.com/properties/prop123/cover.jpg",
            Images = Enumerable.Range(1, 13)
                .Select(i => $"https://0daikfjw6ec1yprw.public.blob.vercel-storage.com/properties/prop123/{i}.jpg")
                .ToArray()
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Images);
    }

    [Test]
    public void Should_reject_invalid_blob_host()
    {
        // Arrange
        var request = new CreatePropertyRequest
        {
            OwnerId = "owner123",
            Name = "Luxury Villa",
            Description = "Luxury villa with modern amenities",
            Address = "123 Ocean Drive",
            City = "Miami Beach",
            PropertyType = "Villa",
            Price = 2500000,
            CodeInternal = "LV001",
            Year = 2020,
            Size = 500,
            Bedrooms = 4,
            Bathrooms = 3,
            CoverImage = "https://invalid-host.com/properties/prop123/cover.jpg"
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CoverImage);
    }

    [Test]
    public void Should_reject_http_urls()
    {
        // Arrange
        var request = new CreatePropertyRequest
        {
            OwnerId = "owner123",
            Name = "Luxury Villa",
            Description = "Luxury villa with modern amenities",
            Address = "123 Ocean Drive",
            City = "Miami Beach",
            PropertyType = "Villa",
            Price = 2500000,
            CodeInternal = "LV001",
            Year = 2020,
            Size = 500,
            Bedrooms = 4,
            Bathrooms = 3,
            CoverImage = "http://store1.public.blob.vercel-storage.com/properties/prop123/cover.jpg"
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CoverImage);
    }

    [Test]
    public void Should_reject_cover_image_with_wrong_path()
    {
        // Arrange
        var request = new CreatePropertyRequest
        {
            OwnerId = "owner123",
            Name = "Luxury Villa",
            Address = "123 Ocean Drive",
            City = "Miami Beach",
            PropertyType = "Villa",
            Price = 2500000,
            CodeInternal = "LV001",
            Year = 2020,
            Size = 500,
            Bedrooms = 4,
            Bathrooms = 3,
            CoverImage = "https://0daikfjw6ec1yprw.public.blob.vercel-storage.com/properties/prop123/1.jpg"
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CoverImage);
    }

    [Test]
    public void Should_reject_gallery_image_with_invalid_index()
    {
        // Arrange
        var request = new CreatePropertyRequest
        {
            OwnerId = "owner123",
            Name = "Luxury Villa",
            Address = "123 Ocean Drive",
            City = "Miami Beach",
            PropertyType = "Villa",
            Price = 2500000,
            CodeInternal = "LV001",
            Year = 2020,
            Size = 500,
            Bedrooms = 4,
            Bathrooms = 3,
            CoverImage = "https://0daikfjw6ec1yprw.public.blob.vercel-storage.com/properties/prop123/cover.jpg",
            Images = new[]
            {
                "https://0daikfjw6ec1yprw.public.blob.vercel-storage.com/properties/prop123/0.jpg", // Invalid: starts at 0
                "https://0daikfjw6ec1yprw.public.blob.vercel-storage.com/properties/prop123/13.jpg" // Invalid: exceeds 12
            }
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Images);
    }

    [Test]
    public void Should_reject_mismatched_property_ids()
    {
        // Arrange
        var request = new CreatePropertyRequest
        {
            OwnerId = "owner123",
            Name = "Luxury Villa",
            Address = "123 Ocean Drive",
            City = "Miami Beach",
            PropertyType = "Villa",
            Price = 2500000,
            CodeInternal = "LV001",
            Year = 2020,
            Size = 500,
            Bedrooms = 4,
            Bathrooms = 3,
            CoverImage = "https://0daikfjw6ec1yprw.public.blob.vercel-storage.com/properties/prop123/cover.jpg",
            Images = new[]
            {
                "https://0daikfjw6ec1yprw.public.blob.vercel-storage.com/properties/prop456/1.jpg" // Different property ID
            }
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x);
    }

    [Test]
    public void Should_reject_negative_price()
    {
        // Arrange
        var request = new CreatePropertyRequest
        {
            OwnerId = "owner123",
            Name = "Luxury Villa",
            Address = "123 Ocean Drive",
            City = "Miami Beach",
            PropertyType = "Villa",
            Price = -1000,
            CodeInternal = "LV001",
            Year = 2020,
            Size = 500,
            Bedrooms = 4,
            Bathrooms = 3,
            CoverImage = "https://0daikfjw6ec1yprw.public.blob.vercel-storage.com/properties/prop123/cover.jpg"
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Price);
    }

    [Test]
    public void Should_reject_name_exceeding_max_length()
    {
        // Arrange
        var request = new CreatePropertyRequest
        {
            OwnerId = "owner123",
            Name = new string('A', 201), // Exceeds 200 characters
            Address = "123 Ocean Drive",
            City = "Miami Beach",
            PropertyType = "Villa",
            Price = 2500000,
            CodeInternal = "LV001",
            Year = 2020,
            Size = 500,
            Bedrooms = 4,
            Bathrooms = 3,
            CoverImage = "https://0daikfjw6ec1yprw.public.blob.vercel-storage.com/properties/prop123/cover.jpg"
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Test]
    public void Should_reject_missing_required_fields()
    {
        // Arrange
        var request = new CreatePropertyRequest
        {
            // Missing OwnerId, Name, Address, City, PropertyType, Price, CodeInternal
            CoverImage = "https://0daikfjw6ec1yprw.public.blob.vercel-storage.com/properties/prop123/cover.jpg"
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.OwnerId);
        result.ShouldHaveValidationErrorFor(x => x.Name);
        result.ShouldHaveValidationErrorFor(x => x.Address);
        result.ShouldHaveValidationErrorFor(x => x.City);
        result.ShouldHaveValidationErrorFor(x => x.PropertyType);
        result.ShouldHaveValidationErrorFor(x => x.CodeInternal);
        // Note: Price validation only occurs when value is provided (must be non-negative)
    }

    [Test]
    public void Should_reject_invalid_year_range()
    {
        // Arrange
        var request = new CreatePropertyRequest
        {
            OwnerId = "owner123",
            Name = "Luxury Villa",
            Address = "123 Ocean Drive",
            City = "Miami Beach",
            PropertyType = "Villa",
            Price = 2500000,
            CodeInternal = "LV001",
            Year = 1799, // Below minimum
            Size = 500,
            Bedrooms = 4,
            Bathrooms = 3,
            CoverImage = "https://0daikfjw6ec1yprw.public.blob.vercel-storage.com/properties/prop123/cover.jpg"
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Year);
    }

    [Test]
    public void Should_reject_negative_size()
    {
        // Arrange
        var request = new CreatePropertyRequest
        {
            OwnerId = "owner123",
            Name = "Luxury Villa",
            Address = "123 Ocean Drive",
            City = "Miami Beach",
            PropertyType = "Villa",
            Price = 2500000,
            CodeInternal = "LV001",
            Year = 2020,
            Size = -100, // Negative size
            Bedrooms = 4,
            Bathrooms = 3,
            CoverImage = "https://0daikfjw6ec1yprw.public.blob.vercel-storage.com/properties/prop123/cover.jpg"
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Size);
    }

    [Test]
    public void Should_reject_invalid_bedroom_count()
    {
        // Arrange
        var request = new CreatePropertyRequest
        {
            OwnerId = "owner123",
            Name = "Luxury Villa",
            Address = "123 Ocean Drive",
            City = "Miami Beach",
            PropertyType = "Villa",
            Price = 2500000,
            CodeInternal = "LV001",
            Year = 2020,
            Size = 500,
            Bedrooms = 21, // Exceeds maximum
            Bathrooms = 3,
            CoverImage = "https://0daikfjw6ec1yprw.public.blob.vercel-storage.com/properties/prop123/cover.jpg"
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Bedrooms);
    }

    [Test]
    public void Should_reject_invalid_bathroom_count()
    {
        // Arrange
        var request = new CreatePropertyRequest
        {
            OwnerId = "owner123",
            Name = "Luxury Villa",
            Address = "123 Ocean Drive",
            City = "Miami Beach",
            PropertyType = "Villa",
            Price = 2500000,
            CodeInternal = "LV001",
            Year = 2020,
            Size = 500,
            Bedrooms = 4,
            Bathrooms = 21, // Exceeds maximum
            CoverImage = "https://0daikfjw6ec1yprw.public.blob.vercel-storage.com/properties/prop123/cover.jpg"
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Bathrooms);
    }

    [Test]
    public void Should_accept_valid_property_with_optional_fields()
    {
        // Arrange
        var request = new CreatePropertyRequest
        {
            OwnerId = "owner123",
            Name = "Luxury Villa",
            Address = "123 Ocean Drive",
            City = "Miami Beach",
            PropertyType = "Villa",
            Price = 2500000,
            CodeInternal = "LV001",
            Year = 2020,
            Size = 500,
            Bedrooms = 4,
            Bathrooms = 3,
            CoverImage = "https://0daikfjw6ec1yprw.public.blob.vercel-storage.com/properties/prop123/cover.jpg",
            Description = "Beautiful luxury villa with ocean view",
            Neighborhood = "South Beach",
            HasPool = true,
            HasGarden = true,
            HasParking = true,
            IsFurnished = false,
            AvailableFrom = DateTime.Now.AddDays(30),
            AvailableTo = DateTime.Now.AddDays(90)
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Test]
    public void Should_accept_valid_property_with_new_media_system()
    {
        // Arrange
        var request = new CreatePropertyRequest
        {
            OwnerId = "owner123",
            Name = "Luxury Villa",
            Address = "123 Ocean Drive",
            City = "Miami Beach",
            PropertyType = "Villa",
            Price = 2500000,
            CodeInternal = "LV001",
            Year = 2020,
            Size = 500,
            Bedrooms = 4,
            Bathrooms = 3,
            Cover = new Domain.Entities.Cover
            {
                Type = Domain.Entities.MediaType.Image,
                Url = "https://0daikfjw6ec1yprw.public.blob.vercel-storage.com/properties/prop123/cover.jpg",
                Index = 0
            },
            Media = new List<Domain.Entities.Media>
            {
                new Domain.Entities.Media
                {
                    Id = "media1",
                    Type = Domain.Entities.MediaType.Image,
                    Url = "https://0daikfjw6ec1yprw.public.blob.vercel-storage.com/properties/prop123/1.jpg",
                    Index = 1,
                    Enabled = true,
                    Featured = true
                },
                new Domain.Entities.Media
                {
                    Id = "media2",
                    Type = Domain.Entities.MediaType.Image,
                    Url = "https://0daikfjw6ec1yprw.public.blob.vercel-storage.com/properties/prop123/2.jpg",
                    Index = 2,
                    Enabled = true,
                    Featured = false
                }
            }
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Test]
    public void Should_reject_property_with_mixed_media_systems()
    {
        // Arrange
        var request = new CreatePropertyRequest
        {
            OwnerId = "owner123",
            Name = "Luxury Villa",
            Address = "123 Ocean Drive",
            City = "Miami Beach",
            PropertyType = "Villa",
            Price = 2500000,
            CodeInternal = "LV001",
            Year = 2020,
            Size = 500,
            Bedrooms = 4,
            Bathrooms = 3,
            CoverImage = "https://0daikfjw6ec1yprw.public.blob.vercel-storage.com/properties/prop123/cover.jpg", // Legacy
            Cover = new Domain.Entities.Cover // New system
            {
                Type = Domain.Entities.MediaType.Image,
                Url = "https://0daikfjw6ec1yprw.public.blob.vercel-storage.com/properties/prop456/cover.jpg", // Different property ID
                Index = 0
            }
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x);
    }

    [Test]
    public void Should_reject_property_with_invalid_media_urls()
    {
        // Arrange
        var request = new CreatePropertyRequest
        {
            OwnerId = "owner123",
            Name = "Luxury Villa",
            Address = "123 Ocean Drive",
            City = "Miami Beach",
            PropertyType = "Villa",
            Price = 2500000,
            CodeInternal = "LV001",
            Year = 2020,
            Size = 500,
            Bedrooms = 4,
            Bathrooms = 3,
            Cover = new Domain.Entities.Cover
            {
                Type = Domain.Entities.MediaType.Image,
                Url = "https://invalid-host.com/properties/prop123/cover.jpg", // Invalid host
                Index = 0
            }
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Cover);
    }

    [Test]
    public void Should_reject_property_with_mismatched_media_property_ids()
    {
        // Arrange
        var request = new CreatePropertyRequest
        {
            OwnerId = "owner123",
            Name = "Luxury Villa",
            Address = "123 Ocean Drive",
            City = "Miami Beach",
            PropertyType = "Villa",
            Price = 2500000,
            CodeInternal = "LV001",
            Year = 2020,
            Size = 500,
            Bedrooms = 4,
            Bathrooms = 3,
            Cover = new Domain.Entities.Cover
            {
                Type = Domain.Entities.MediaType.Image,
                Url = "https://0daikfjw6ec1yprw.public.blob.vercel-storage.com/properties/prop123/cover.jpg",
                Index = 0
            },
            Media = new List<Domain.Entities.Media>
            {
                new Domain.Entities.Media
                {
                    Id = "media1",
                    Type = Domain.Entities.MediaType.Image,
                    Url = "https://0daikfjw6ec1yprw.public.blob.vercel-storage.com/properties/prop456/1.jpg", // Different property ID
                    Index = 1,
                    Enabled = true,
                    Featured = true
                }
            }
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x);
    }

    [Test]
    public void Should_accept_property_with_only_new_media_system()
    {
        // Arrange
        var request = new CreatePropertyRequest
        {
            OwnerId = "owner123",
            Name = "Luxury Villa",
            Address = "123 Ocean Drive",
            City = "Miami Beach",
            PropertyType = "Villa",
            Price = 2500000,
            CodeInternal = "LV001",
            Year = 2020,
            Size = 500,
            Bedrooms = 4,
            Bathrooms = 3,
            Cover = new Domain.Entities.Cover
            {
                Type = Domain.Entities.MediaType.Image,
                Url = "https://0daikfjw6ec1yprw.public.blob.vercel-storage.com/properties/prop123/cover.jpg",
                Index = 0
            }
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
