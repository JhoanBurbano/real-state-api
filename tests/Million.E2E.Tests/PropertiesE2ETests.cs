using FluentAssertions;
using NUnit.Framework;
using Newtonsoft.Json;

namespace Million.E2E.Tests;

[TestFixture]
public class PropertiesE2ETests : TestBase
{
    [Test]
    public async Task GetProperties_WithoutAuthentication_ShouldReturnUnauthorized()
    {
        // Act
        var response = await Client.GetAsync("/properties");

        // Assert
        response.Should().HaveStatusCode(System.Net.HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task GetProperties_WithAuthentication_ShouldReturnProperties()
    {
        // Arrange
        var ownerClient = await CreateOwnerClientAsync();

        // Act
        var response = await ownerClient.GetAsync("/properties");

        // Assert
        response.Should().BeSuccessful();
        
        var propertiesResponse = await DeserializeResponseAsync<dynamic>(response);
        propertiesResponse.Should().NotBeNull();
        propertiesResponse!.items.Should().NotBeNull();
        propertiesResponse.total.Should().Be(50);
        propertiesResponse.page.Should().Be(1);
        propertiesResponse.pageSize.Should().Be(20);
    }

    [Test]
    public async Task GetProperties_WithPagination_ShouldReturnCorrectPage()
    {
        // Arrange
        var ownerClient = await CreateOwnerClientAsync();

        // Act
        var response = await ownerClient.GetAsync("/properties?page=2&pageSize=10");

        // Assert
        response.Should().BeSuccessful();
        
        var propertiesResponse = await DeserializeResponseAsync<dynamic>(response);
        propertiesResponse.Should().NotBeNull();
        propertiesResponse!.page.Should().Be(2);
        propertiesResponse.pageSize.Should().Be(10);
        propertiesResponse.items.Should().HaveCount(10);
    }

    [Test]
    public async Task GetProperties_WithPriceFilter_ShouldReturnFilteredResults()
    {
        // Arrange
        var ownerClient = await CreateOwnerClientAsync();

        // Act
        var response = await ownerClient.GetAsync("/properties?minPrice=5000000&maxPrice=10000000");

        // Assert
        response.Should().BeSuccessful();
        
        var propertiesResponse = await DeserializeResponseAsync<dynamic>(response);
        propertiesResponse.Should().NotBeNull();
        
        // Verify all returned properties are within price range
        foreach (var property in propertiesResponse!.items)
        {
            var price = Convert.ToDecimal(property.price);
            price.Should().BeGreaterThanOrEqualTo(5000000);
            price.Should().BeLessThanOrEqualTo(10000000);
        }
    }

    [Test]
    public async Task GetProperties_WithCityFilter_ShouldReturnFilteredResults()
    {
        // Arrange
        var ownerClient = await CreateOwnerClientAsync();

        // Act
        var response = await ownerClient.GetAsync("/properties?city=Miami Beach");

        // Assert
        response.Should().BeSuccessful();
        
        var propertiesResponse = await DeserializeResponseAsync<dynamic>(response);
        propertiesResponse.Should().NotBeNull();
        
        // Verify all returned properties are in Miami Beach
        foreach (var property in propertiesResponse!.items)
        {
            property.city.Should().Be("Miami Beach");
        }
    }

    [Test]
    public async Task GetProperties_WithBedroomsFilter_ShouldReturnFilteredResults()
    {
        // Arrange
        var ownerClient = await CreateOwnerClientAsync();

        // Act
        var response = await ownerClient.GetAsync("/properties?bedrooms=4");

        // Assert
        response.Should().BeSuccessful();
        
        var propertiesResponse = await DeserializeResponseAsync<dynamic>(response);
        propertiesResponse.Should().NotBeNull();
        
        // Verify all returned properties have 4 bedrooms
        foreach (var property in propertiesResponse!.items)
        {
            property.bedrooms.Should().Be(4);
        }
    }

    [Test]
    public async Task GetProperties_WithSorting_ShouldReturnSortedResults()
    {
        // Arrange
        var ownerClient = await CreateOwnerClientAsync();

        // Act
        var response = await ownerClient.GetAsync("/properties?sort=price_desc");

        // Assert
        response.Should().BeSuccessful();
        
        var propertiesResponse = await DeserializeResponseAsync<dynamic>(response);
        propertiesResponse.Should().NotBeNull();
        
        // Verify properties are sorted by price descending
        var prices = new List<decimal>();
        foreach (var property in propertiesResponse!.items)
        {
            prices.Add(Convert.ToDecimal(property.price));
        }
        
        prices.Should().BeInDescendingOrder();
    }

    [Test]
    public async Task GetPropertyById_WithValidId_ShouldReturnProperty()
    {
        // Arrange
        var ownerClient = await CreateOwnerClientAsync();

        // Act
        var response = await ownerClient.GetAsync("/properties/prop-001");

        // Assert
        response.Should().BeSuccessful();
        
        var property = await DeserializeResponseAsync<dynamic>(response);
        property.Should().NotBeNull();
        property!.id.Should().Be("prop-001");
        property.name.Should().Be("Penthouse 1 - Coral Gables");
        property.city.Should().Be("Coral Gables");
        property.media.Should().NotBeNull();
        property.cover.Should().NotBeNull();
    }

    [Test]
    public async Task GetPropertyById_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        var ownerClient = await CreateOwnerClientAsync();

        // Act
        var response = await ownerClient.GetAsync("/properties/invalid-id");

        // Assert
        response.Should().HaveStatusCode(System.Net.HttpStatusCode.NotFound);
        
        var errorResponse = await DeserializeResponseAsync<dynamic>(response);
        errorResponse.Should().NotBeNull();
        errorResponse!.title.Should().Be("Property Not Found");
    }

    [Test]
    public async Task CreateProperty_WithValidData_ShouldReturnCreatedProperty()
    {
        // Arrange
        var ownerClient = await CreateOwnerClientAsync();
        var createRequest = new
        {
            OwnerId = "owner-001",
            Name = "Test Property",
            Address = "123 Test St, Miami, FL",
            Price = 2500000,
            CodeInternal = "TEST001",
            Year = 2023,
            Description = "A test property for e2e testing",
            City = "Miami",
            Neighborhood = "Downtown",
            PropertyType = "Condo",
            Size = 2000,
            Bedrooms = 3,
            Bathrooms = 2,
            HasPool = true,
            HasGarden = false,
            HasParking = true,
            IsFurnished = true,
            CoverImage = "https://blob.vercel-storage.com/properties/test001/cover.jpg",
            Images = new[]
            {
                "https://blob.vercel-storage.com/properties/test001/1.jpg",
                "https://blob.vercel-storage.com/properties/test001/2.jpg"
            }
        };

        // Act
        var response = await ownerClient.PostAsJsonAsync("/properties", createRequest);

        // Assert
        response.Should().BeSuccessful();
        
        var createdProperty = await DeserializeResponseAsync<dynamic>(response);
        createdProperty.Should().NotBeNull();
        createdProperty!.name.Should().Be("Test Property");
        createdProperty.price.Should().Be(2500000);
        createdProperty.city.Should().Be("Miami");
        createdProperty.ownerId.Should().Be("owner-001");
    }

    [Test]
    public async Task CreateProperty_WithInvalidData_ShouldReturnBadRequest()
    {
        // Arrange
        var ownerClient = await CreateOwnerClientAsync();
        var createRequest = new
        {
            OwnerId = "", // Invalid: empty owner ID
            Name = "", // Invalid: empty name
            Price = -1000, // Invalid: negative price
            CodeInternal = "TEST002",
            Year = 2023
        };

        // Act
        var response = await ownerClient.PostAsJsonAsync("/properties", createRequest);

        // Assert
        response.Should().HaveStatusCode(System.Net.HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task UpdateProperty_WithValidData_ShouldReturnUpdatedProperty()
    {
        // Arrange
        var ownerClient = await CreateOwnerClientAsync();
        var updateRequest = new
        {
            Name = "Updated Property Name",
            Price = 3000000,
            Description = "Updated description"
        };

        // Act
        var response = await ownerClient.PutAsJsonAsync("/properties/prop-001", updateRequest);

        // Assert
        response.Should().BeSuccessful();
        
        var updatedProperty = await DeserializeResponseAsync<dynamic>(response);
        updatedProperty.Should().NotBeNull();
        updatedProperty!.name.Should().Be("Updated Property Name");
        updatedProperty.price.Should().Be(3000000);
        updatedProperty.description.Should().Be("Updated description");
    }

    [Test]
    public async Task UpdateProperty_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        var ownerClient = await CreateOwnerClientAsync();
        var updateRequest = new
        {
            Name = "Updated Property Name"
        };

        // Act
        var response = await ownerClient.PutAsJsonAsync("/properties/invalid-id", updateRequest);

        // Assert
        response.Should().HaveStatusCode(System.Net.HttpStatusCode.NotFound);
    }

    [Test]
    public async Task DeleteProperty_WithValidId_ShouldReturnSuccess()
    {
        // Arrange
        var ownerClient = await CreateOwnerClientAsync();

        // Act
        var response = await ownerClient.DeleteAsync("/properties/prop-002");

        // Assert
        response.Should().BeSuccessful();
        
        // Verify property is deleted
        var getResponse = await ownerClient.GetAsync("/properties/prop-002");
        getResponse.Should().HaveStatusCode(System.Net.HttpStatusCode.NotFound);
    }

    [Test]
    public async Task DeleteProperty_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        var ownerClient = await CreateOwnerClientAsync();

        // Act
        var response = await ownerClient.DeleteAsync("/properties/invalid-id");

        // Assert
        response.Should().HaveStatusCode(System.Net.HttpStatusCode.NotFound);
    }

    [Test]
    public async Task ActivateProperty_WithValidId_ShouldReturnSuccess()
    {
        // Arrange
        var ownerClient = await CreateOwnerClientAsync();

        // Act
        var response = await ownerClient.PatchAsync("/properties/prop-003/activate", null);

        // Assert
        response.Should().BeSuccessful();
    }

    [Test]
    public async Task DeactivateProperty_WithValidId_ShouldReturnSuccess()
    {
        // Arrange
        var ownerClient = await CreateOwnerClientAsync();

        // Act
        var response = await ownerClient.PatchAsync("/properties/prop-004/deactivate", null);

        // Assert
        response.Should().BeSuccessful();
    }

    [Test]
    public async Task GetPropertyMedia_WithValidId_ShouldReturnMedia()
    {
        // Arrange
        var ownerClient = await CreateOwnerClientAsync();

        // Act
        var response = await ownerClient.GetAsync("/properties/prop-001/media");

        // Assert
        response.Should().BeSuccessful();
        
        var mediaResponse = await DeserializeResponseAsync<dynamic>(response);
        mediaResponse.Should().NotBeNull();
        mediaResponse.Should().HaveCountGreaterThan(0);
    }

    [Test]
    public async Task UpdatePropertyMedia_WithValidData_ShouldReturnSuccess()
    {
        // Arrange
        var ownerClient = await CreateOwnerClientAsync();
        var mediaPatch = new
        {
            MediaId = "media-1-1",
            Index = 5,
            Featured = false,
            Enabled = true
        };

        // Act
        var response = await ownerClient.PatchAsJsonAsync("/properties/prop-001/media", mediaPatch);

        // Assert
        response.Should().BeSuccessful();
    }

    [Test]
    public async Task AddPropertyTrace_WithValidData_ShouldReturnSuccess()
    {
        // Arrange
        var ownerClient = await CreateOwnerClientAsync();
        var traceRequest = new
        {
            DateSale = DateTime.UtcNow.AddDays(-30),
            Name = "Test Sale",
            Value = 2000000,
            Tax = 100000
        };

        // Act
        var response = await ownerClient.PostAsJsonAsync("/properties/prop-001/traces", traceRequest);

        // Assert
        response.Should().BeSuccessful();
    }

    [Test]
    public async Task SearchProperties_WithTextQuery_ShouldReturnResults()
    {
        // Arrange
        var ownerClient = await CreateOwnerClientAsync();

        // Act
        var response = await ownerClient.GetAsync("/properties?query=villa");

        // Assert
        response.Should().BeSuccessful();
        
        var propertiesResponse = await DeserializeResponseAsync<dynamic>(response);
        propertiesResponse.Should().NotBeNull();
        
        // Verify at least one property contains "villa" in name or address
        bool hasVillaProperty = false;
        foreach (var property in propertiesResponse!.items)
        {
            var name = property.name.ToString().ToLower();
            var address = property.address.ToString().ToLower();
            if (name.Contains("villa") || address.Contains("villa"))
            {
                hasVillaProperty = true;
                break;
            }
        }
        hasVillaProperty.Should().BeTrue();
    }
}

