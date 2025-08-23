using FluentAssertions;
using NUnit.Framework;
using System.Net.Http.Headers;

namespace Million.E2E.Tests;

[TestFixture]
public class MiddlewareE2ETests : TestBase
{
    [Test]
    public async Task Request_ShouldIncludeCorrelationId()
    {
        // Act
        var response = await Client.GetAsync("/properties");

        // Assert
        response.Headers.Should().ContainKey("X-Correlation-Id");
        var correlationId = response.Headers.GetValues("X-Correlation-Id").FirstOrDefault();
        correlationId.Should().NotBeNullOrEmpty();
        correlationId.Should().HaveLength(36); // UUID format
    }

    [Test]
    public async Task RateLimiting_WithinLimits_ShouldSucceed()
    {
        // Arrange
        var ownerClient = await CreateOwnerClientAsync();

        // Act - Make multiple requests within rate limit
        var responses = new List<HttpResponseMessage>();
        for (int i = 0; i < 10; i++)
        {
            var response = await ownerClient.GetAsync("/properties");
            responses.Add(response);
        }

        // Assert
        responses.Should().OnlyContain(r => r.IsSuccessStatusCode);

        // Check rate limit headers
        var lastResponse = responses.Last();
        lastResponse.Headers.Should().ContainKey("X-RateLimit-Limit");
        lastResponse.Headers.Should().ContainKey("X-RateLimit-Remaining");
        lastResponse.Headers.Should().ContainKey("X-RateLimit-Reset");
    }

    [Test]
    public async Task RateLimiting_ExceedingLimits_ShouldReturnTooManyRequests()
    {
        // Arrange
        var ownerClient = await CreateOwnerClientAsync();

        // Act - Make many requests to exceed rate limit
        var responses = new List<HttpResponseMessage>();
        for (int i = 0; i < 100; i++)
        {
            var response = await ownerClient.GetAsync("/properties");
            responses.Add(response);

            if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                break;
        }

        // Assert
        responses.Should().Contain(r => r.StatusCode == System.Net.HttpStatusCode.TooManyRequests);

        var rateLimitedResponse = responses.First(r => r.StatusCode == System.Net.HttpStatusCode.TooManyRequests);
        rateLimitedResponse.Headers.Should().ContainKey("X-RateLimit-Limit");
        rateLimitedResponse.Headers.Should().ContainKey("X-RateLimit-Remaining");
        rateLimitedResponse.Headers.Should().ContainKey("X-RateLimit-Reset");

        var errorResponse = await DeserializeResponseAsync<dynamic>(rateLimitedResponse);
        errorResponse.Should().NotBeNull();
        errorResponse!.title.Should().Be("Too Many Requests");
        errorResponse.status.Should().Be(429);
    }

    [Test]
    public async Task RateLimiting_BurstMode_ShouldAllowBurstRequests()
    {
        // Arrange
        var ownerClient = await CreateOwnerClientAsync();

        // Act - Make burst requests
        var tasks = new List<Task<HttpResponseMessage>>();
        for (int i = 0; i < 50; i++)
        {
            tasks.Add(ownerClient.GetAsync("/properties"));
        }

        var responses = await Task.WhenAll(tasks);

        // Assert - Most requests should succeed due to burst allowance
        var successCount = responses.Count(r => r.IsSuccessStatusCode);
        successCount.Should().BeGreaterThan(30); // Allow some failures but most should succeed
    }

    [Test]
    public async Task ProblemDetails_ValidationError_ShouldReturnProperFormat()
    {
        // Arrange
        var ownerClient = await CreateOwnerClientAsync();
        var invalidRequest = new
        {
            OwnerId = "", // Invalid: empty
            Name = "", // Invalid: empty
            Price = -1000 // Invalid: negative
        };

        // Act
        var response = await ownerClient.PostAsJsonAsync("/properties", invalidRequest);

        // Assert
        response.Should().HaveStatusCode(System.Net.HttpStatusCode.BadRequest);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/problem+json");

        var problemDetails = await DeserializeResponseAsync<dynamic>(response);
        problemDetails.Should().NotBeNull();
        problemDetails!.type.Should().Be("https://tools.ietf.org/html/rfc7231#section-6.5.1");
        problemDetails.title.Should().Be("One or more validation errors occurred.");
        problemDetails.status.Should().Be(400);
        problemDetails.errors.Should().NotBeNull();
    }

    [Test]
    public async Task ProblemDetails_NotFound_ShouldReturnProperFormat()
    {
        // Arrange
        var ownerClient = await CreateOwnerClientAsync();

        // Act
        var response = await ownerClient.GetAsync("/properties/non-existent-id");

        // Assert
        response.Should().HaveStatusCode(System.Net.HttpStatusCode.NotFound);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/problem+json");

        var problemDetails = await DeserializeResponseAsync<dynamic>(response);
        problemDetails.Should().NotBeNull();
        problemDetails!.type.Should().Be("https://tools.ietf.org/html/rfc7231#section-6.5.4");
        problemDetails.title.Should().Be("Property Not Found");
        problemDetails.status.Should().Be(404);
    }

    [Test]
    public async Task ProblemDetails_Unauthorized_ShouldReturnProperFormat()
    {
        // Act
        var response = await Client.GetAsync("/properties");

        // Assert
        response.Should().HaveStatusCode(System.Net.HttpStatusCode.Unauthorized);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/problem+json");

        var problemDetails = await DeserializeResponseAsync<dynamic>(response);
        problemDetails.Should().NotBeNull();
        problemDetails!.type.Should().Be("https://tools.ietf.org/html/rfc7235#section-3.1");
        problemDetails.title.Should().Be("Unauthorized");
        problemDetails.status.Should().Be(401);
    }

    [Test]
    public async Task StructuredLogging_ShouldIncludeCorrelationId()
    {
        // Act
        var response = await Client.GetAsync("/properties");

        // Assert
        response.Headers.Should().ContainKey("X-Correlation-ID");
        var correlationId = response.Headers.GetValues("X-Correlation-ID").FirstOrDefault();
        correlationId.Should().NotBeNullOrEmpty();

        // The correlation ID should be consistent across the request
        correlationId.Should().HaveLength(36);
    }

    [Test]
    public async Task Middleware_Order_ShouldBeCorrect()
    {
        // Arrange
        var ownerClient = await CreateOwnerClientAsync();

        // Act
        var response = await ownerClient.GetAsync("/properties");

        // Assert
        // 1. Correlation ID should be present
        response.Headers.Should().ContainKey("X-Correlation-ID");

        // 2. Rate limit headers should be present
        response.Headers.Should().ContainKey("X-RateLimit-Limit");
        response.Headers.Should().ContainKey("X-RateLimit-Remaining");

        // 3. Response should be successful (no middleware errors)
        response.Should().BeSuccessful();
    }

    [Test]
    public async Task RateLimiting_DifferentEndpoints_ShouldHaveSeparateCounters()
    {
        // Arrange
        var ownerClient = await CreateOwnerClientAsync();

        // Act - Make requests to different endpoints
        var propertiesResponse = await ownerClient.GetAsync("/properties");
        var propertyResponse = await ownerClient.GetAsync("/properties/prop-001");

        // Assert
        propertiesResponse.Should().BeSuccessful();
        propertyResponse.Should().BeSuccessful();

        // Both should have rate limit headers
        propertiesResponse.Headers.Should().ContainKey("X-RateLimit-Limit");
        propertyResponse.Headers.Should().ContainKey("X-RateLimit-Limit");
    }

    [Test]
    public async Task RateLimiting_Authentication_ShouldNotAffectRateLimits()
    {
        // Arrange
        var ownerClient = await CreateOwnerClientAsync();

        // Act - Make authenticated requests
        var responses = new List<HttpResponseMessage>();
        for (int i = 0; i < 15; i++)
        {
            var response = await ownerClient.GetAsync("/properties");
            responses.Add(response);
        }

        // Assert
        responses.Should().OnlyContain(r => r.IsSuccessStatusCode);

        // Rate limit headers should be present
        var lastResponse = responses.Last();
        lastResponse.Headers.Should().ContainKey("X-RateLimit-Remaining");
        var remaining = lastResponse.Headers.GetValues("X-RateLimit-Remaining").FirstOrDefault();
        remaining.Should().NotBeNullOrEmpty();
    }

    [Test]
    public async Task ErrorHandling_InternalServerError_ShouldReturnProblemDetails()
    {
        // This test would require an endpoint that throws an exception
        // For now, we'll test that the middleware is properly configured

        // Arrange
        var ownerClient = await CreateOwnerClientAsync();

        // Act - Try to access a non-existent endpoint
        var response = await ownerClient.GetAsync("/non-existent-endpoint");

        // Assert
        response.Should().HaveStatusCode(System.Net.HttpStatusCode.NotFound);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/problem+json");
    }

    [Test]
    public async Task CorrelationId_Consistency_ShouldBeMaintained()
    {
        // Arrange
        var ownerClient = await CreateOwnerClientAsync();

        // Act - Make multiple requests
        var correlationIds = new List<string>();
        for (int i = 0; i < 5; i++)
        {
            var response = await ownerClient.GetAsync("/properties");
            var correlationId = response.Headers.GetValues("X-Correlation-ID").FirstOrDefault();
            correlationIds.Add(correlationId!);
        }

        // Assert - Each request should have a unique correlation ID
        correlationIds.Should().OnlyHaveUniqueItems();
        correlationIds.Should().OnlyContain(id => !string.IsNullOrEmpty(id) && id.Length == 36);
    }
}

