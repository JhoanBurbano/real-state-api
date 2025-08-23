# Million Properties API - Architecture Documentation 🏗️

## Overview

The Million Properties API is built using **Clean Architecture** principles with a **Domain-Driven Design** approach, providing a robust foundation for luxury real estate property management with advanced search capabilities and image management.

## 🏛️ Architecture Layers

### **1. Domain Layer (`Million.Domain`)**
**Core business entities and domain logic**

```csharp
// Core Property Entity
public class Property
{
    public string Id { get; set; }
    public string IdOwner { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string AddressProperty { get; set; }
    public string City { get; set; }
    public string Neighborhood { get; set; }
    public string PropertyType { get; set; }
    public decimal PriceProperty { get; set; }
    public decimal Size { get; set; }
    public int Bedrooms { get; set; }
    public int Bathrooms { get; set; }
    public bool HasPool { get; set; }
    public bool HasGarden { get; set; }
    public bool HasParking { get; set; }
    public bool IsFurnished { get; set; }
    public DateTime AvailableFrom { get; set; }
    public DateTime AvailableTo { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string Image { get; set; }
    public string[] Images { get; set; }
    public bool IsActive { get; set; }
}
```

**Key Characteristics:**
- ✅ **Pure Domain Logic** - No external dependencies
- ✅ **Business Rules** - Encapsulated validation logic
- ✅ **Immutable Core** - Stable business model
- ✅ **Rich Domain Model** - Behavior and data together

### **2. Application Layer (`Million.Application`)**
**Use cases, business logic, and application services**

#### **Services**
```csharp
public interface IPropertyService
{
    Task<PagedResult<PropertyDto>> GetPropertiesAsync(PropertyListQuery query, CancellationToken ct);
    Task<PropertyDto?> GetByIdAsync(string id, CancellationToken ct);
    Task<PropertyDto> CreatePropertyAsync(CreatePropertyRequest request, CancellationToken ct);
    Task<PropertyDto> UpdatePropertyAsync(string id, UpdatePropertyRequest request, CancellationToken ct);
    Task<bool> DeletePropertyAsync(string id, CancellationToken ct);
    Task<bool> ActivatePropertyAsync(string id, CancellationToken ct);
    Task<bool> DeactivatePropertyAsync(string id, CancellationToken ct);
}
```

#### **DTOs (Data Transfer Objects)**
```csharp
// Request DTOs
public class CreatePropertyRequest { /* Property creation data */ }
public class UpdatePropertyRequest { /* Property update data */ }
public class PropertyListQuery { /* Search and filter parameters */ }

// Response DTOs
public class PropertyDto { /* Property response data */ }
public class PagedResult<T> { /* Paginated results */ }
```

#### **Validation**
```csharp
public class CreatePropertyRequestValidator : AbstractValidator<CreatePropertyRequest>
{
    // Comprehensive validation rules for property creation
    // Including Vercel Blob URL validation
}

public class UpdatePropertyRequestValidator : AbstractValidator<UpdatePropertyRequest>
{
    // Conditional validation for partial updates
}

public class PropertyListQueryValidator : AbstractValidator<PropertyListQuery>
{
    // Advanced filtering and sorting validation
}
```

**Key Characteristics:**
- ✅ **Use Case Orchestration** - Business workflow coordination
- ✅ **Input Validation** - FluentValidation integration
- ✅ **DTO Mapping** - Clean data transformation
- ✅ **Business Rules** - Domain logic enforcement

### **3. Infrastructure Layer (`Million.Infrastructure`)**
**External concerns: database, external services, configuration**

#### **Data Persistence**
```csharp
public class PropertyDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }
    
    // MongoDB document representation
    // Includes FromEntity() and ToEntity() mapping methods
}

public class PropertyRepository : IPropertyRepository
{
    // MongoDB implementation with advanced filtering
    // Full CRUD operations support
    // Complex query building for search and filtering
}
```

#### **MongoDB Context**
```csharp
public class MongoContext
{
    public IMongoDatabase Database { get; }
    public IMongoCollection<T> GetCollection<T>(string name);
}
```

#### **Configuration**
```csharp
public class MongoOptions
{
    public string Uri { get; set; }
    public string Database { get; set; }
}
```

**Key Characteristics:**
- ✅ **Data Access** - MongoDB with proper ObjectId mapping
- ✅ **External Services** - Vercel Blob integration
- ✅ **Configuration** - Environment-based settings
- ✅ **Data Mapping** - Document-Entity conversion

### **4. Web Layer (`Million.Web`)**
**HTTP API, middleware, and cross-cutting concerns**

#### **Minimal API Endpoints**
```csharp
// CRUD Operations
app.MapGet("/properties", async (PropertyListQuery query, ...) => { /* List with filters */ });
app.MapGet("/properties/{id}", async (string id, ...) => { /* Get by ID */ });
app.MapPost("/properties", async (CreatePropertyRequest request, ...) => { /* Create */ });
app.MapPut("/properties/{id}", async (string id, UpdatePropertyRequest request, ...) => { /* Update */ });
app.MapDelete("/properties/{id}", async (string id, ...) => { /* Delete */ });

// Activation Operations
app.MapPatch("/properties/{id}/activate", async (string id, ...) => { /* Activate */ });
app.MapPatch("/properties/{id}/deactivate", async (string id, ...) => { /* Deactivate */ });
```

#### **Middleware Stack**
```csharp
app.UseMiddleware<CorrelationIdMiddleware>();      // Request tracing
app.UseMiddleware<ProblemDetailsMiddleware>();     // Error handling
app.UseSerilogRequestLogging();                    // Request logging
app.UseCors("Allowlist");                         // CORS policy
```

#### **Cross-Cutting Concerns**
- **Rate Limiting** - Per-IP request throttling
- **CORS** - Configurable cross-origin access
- **Validation** - Automatic FluentValidation integration
- **Swagger** - Interactive API documentation

**Key Characteristics:**
- ✅ **HTTP API** - RESTful endpoints with proper status codes
- ✅ **Middleware** - Cross-cutting concerns separation
- ✅ **Error Handling** - RFC 7807 ProblemDetails
- ✅ **Documentation** - Swagger/OpenAPI integration

## 🔄 Data Flow

### **Request Processing Flow**
```
HTTP Request → Middleware Stack → Validation → Service Layer → Repository → MongoDB
                ↓
            Response ← DTO Mapping ← Business Logic ← Data Mapping ← Document
```

### **CRUD Operations Flow**

#### **Create Property**
```
POST /properties
├── Validate CreatePropertyRequest
├── Map to Property Entity
├── Set timestamps and defaults
├── Save to MongoDB
└── Return PropertyDto
```

#### **Read Properties (Advanced Search)**
```
GET /properties?search=miami&minPrice=1000000&sort=-price
├── Validate PropertyListQuery
├── Build MongoDB filters
├── Apply search, filtering, sorting
├── Execute paginated query
├── Map documents to entities
└── Return PagedResult<PropertyDto>
```

#### **Update Property**
```
PUT /properties/{id}
├── Validate UpdatePropertyRequest
├── Find existing property
├── Apply partial updates
├── Update timestamp
├── Save to MongoDB
└── Return updated PropertyDto
```

## 🗄️ Database Design

### **MongoDB Collections**

#### **Properties Collection**
```json
{
  "_id": "ObjectId('68a6ad80c9d00d40aa74e39a')",
  "idOwner": "owner123",
  "name": "Luxury Villa in Miami Beach",
  "description": "Stunning luxury villa with ocean views...",
  "addressProperty": "123 Ocean Drive, Miami Beach, FL 33139",
  "city": "Miami Beach",
  "neighborhood": "South Beach",
  "propertyType": "Villa",
  "priceProperty": 2500000.00,
  "size": 350.0,
  "bedrooms": 4,
  "bathrooms": 3,
  "hasPool": true,
  "hasGarden": true,
  "hasParking": true,
  "isFurnished": true,
  "availableFrom": "2024-01-01T00:00:00Z",
  "availableTo": "2024-12-31T23:59:59Z",
  "createdAt": "2024-01-15T10:30:00Z",
  "updatedAt": "2024-01-15T10:30:00Z",
  "image": "https://store1.public.blob.vercel-storage.com/properties/prop123/cover.jpg",
  "images": [
    "https://store1.public.blob.vercel-storage.com/properties/prop123/1.jpg",
    "https://store1.public.blob.vercel-storage.com/properties/prop123/2.jpg"
  ],
  "isActive": true
}
```

### **Indexes**
```javascript
// Price-based queries
db.properties.createIndex({ "priceProperty": 1 });

// Text search across multiple fields
db.properties.createIndex({
  "name": "text",
  "description": "text",
  "addressProperty": "text",
  "city": "text",
  "neighborhood": "text"
});

// Location-based queries
db.properties.createIndex({ "city": 1, "neighborhood": 1 });

// Property features
db.properties.createIndex({ "bedrooms": 1, "bathrooms": 1 });

// Availability queries
db.properties.createIndex({ "availableFrom": 1, "availableTo": 1 });

// Owner filtering
db.properties.createIndex({ "idOwner": 1 });

// Active properties (default filter)
db.properties.createIndex({ "isActive": 1 });
```

## 🔍 Search & Filtering Architecture

### **Filter Building Strategy**
```csharp
public async Task<(IReadOnlyList<Property> Items, long Total)> FindAsync(PropertyListQuery query, CancellationToken cancellationToken)
{
    var filters = new List<FilterDefinition<PropertyDocument>>();
    var builder = Builders<PropertyDocument>.Filter;

    // Dynamic filter building based on query parameters
    if (!string.IsNullOrWhiteSpace(query.Search))
    {
        var searchFilter = builder.Or(
            builder.Regex(x => x.Name, new BsonRegularExpression(query.Search, "i")),
            builder.Regex(x => x.Description, new BsonRegularExpression(query.Search, "i")),
            builder.Regex(x => x.AddressProperty, new BsonRegularExpression(query.Search, "i")),
            builder.Regex(x => x.City, new BsonRegularExpression(query.Search, "i")),
            builder.Regex(x => x.Neighborhood, new BsonRegularExpression(query.Search, "i"))
        );
        filters.Add(searchFilter);
    }

    // Add other filters...
    var filter = filters.Count > 0 ? builder.And(filters) : builder.Empty;
    
    // Apply sorting and pagination
    var sort = ParseSort(query.Sort);
    var skip = (query.Page - 1) * query.PageSize;
    
    // Execute query with pagination
    var totalTask = _collection.CountDocumentsAsync(filter, cancellationToken: cancellationToken);
    var itemsTask = _collection.Find(filter).Sort(sort).Skip(skip).Limit(query.PageSize).ToListAsync(cancellationToken);
    
    await Task.WhenAll(totalTask, itemsTask);
    return (itemsTask.Result.Select(doc => doc.ToEntity()).ToList(), totalTask.Result);
}
```

### **Sorting Strategy**
```csharp
private static SortDefinition<PropertyDocument> ParseSort(string? sort)
{
    var builder = Builders<PropertyDocument>.Sort;
    return (sort?.ToLowerInvariant()) switch
    {
        "price" => builder.Ascending(x => x.PriceProperty),
        "-price" => builder.Descending(x => x.PriceProperty),
        "name" => builder.Ascending(x => x.Name),
        "-name" => builder.Descending(x => x.Name),
        "date" => builder.Ascending(x => x.CreatedAt),
        "-date" => builder.Descending(x => x.CreatedAt),
        "size" => builder.Ascending(x => x.Size),
        "-size" => builder.Descending(x => x.Size),
        "bedrooms" => builder.Ascending(x => x.Bedrooms),
        "-bedrooms" => builder.Descending(x => x.Bedrooms),
        "bathrooms" => builder.Ascending(x => x.Bathrooms),
        "-bathrooms" => builder.Descending(x => x.Bathrooms),
        _ => builder.Descending(x => x.PriceProperty) // Default sort
    };
}
```

## 🖼️ Image Management Architecture

### **Vercel Blob Integration**
```typescript
// Next.js API Route
export async function POST(request: NextRequest) {
  const { propertyId, kind, index } = await request.json();
  
  // Validate inputs
  if (!propertyId || !['cover', 'gallery'].includes(kind)) {
    return NextResponse.json({ error: 'Invalid parameters' }, { status: 400 });
  }
  
  // Compute pathname
  let pathname: string;
  if (kind === 'cover') {
    pathname = `properties/${propertyId}/cover`;
  } else {
    pathname = `properties/${propertyId}/${index}`;
  }
  
  // Get upload URL from Vercel Blob
  const { url, pathname: blobPathname, contentType } = await handleUpload({
    pathname,
    clientPayload: { propertyId, kind, index },
    access: 'public',
  });
  
  return NextResponse.json({ url, pathname: blobPathname, contentType });
}
```

### **URL Validation Strategy**
```csharp
private static readonly Regex BlobUrlRegex = new(
    @"^https://[a-z0-9.-]+\.public\.blob\.vercel-storage\.com/properties/([a-zA-Z0-9]+)/(cover|([1-9]|1[0-2]))\.[a-zA-Z0-9]+$",
    RegexOptions.Compiled | RegexOptions.IgnoreCase);

private static bool BeValidBlobUrl(string url)
{
    if (string.IsNullOrEmpty(url)) return false;
    return BlobUrlRegex.IsMatch(url);
}
```

## 🧪 Testing Architecture

### **Test Structure**
```
Million.Tests/
├── PropertyCreateValidatorTests.cs      // Validation logic tests
├── PropertyListQueryValidatorTests.cs   // Query validation tests
├── PropertiesControllerTests.cs         // Service layer tests
├── PropertyServiceTests.cs              // Business logic tests
├── PropertyRepositoryFilterBuilderTests.cs // Data access tests
├── CorrelationIdMiddlewareTests.cs      // Middleware tests
└── ProblemDetailsMiddlewareTests.cs     // Error handling tests
```

### **Testing Strategy**
- **Unit Tests** - Individual component testing
- **Integration Tests** - Component interaction testing
- **Validation Tests** - Business rule enforcement
- **Middleware Tests** - Cross-cutting concerns
- **Mock Usage** - NSubstitute for dependency isolation

## 🔒 Security Architecture

### **Input Validation**
- **FluentValidation** - Comprehensive request validation
- **Data Annotations** - Model-level validation
- **Custom Validators** - Business rule enforcement
- **URL Security** - Strict Vercel Blob format validation

### **Rate Limiting**
```csharp
public class SimpleRateLimiter
{
    private readonly IMemoryCache _cache;
    
    public bool Allow(string clientIp, int limitPerMinute)
    {
        var key = $"rate_limit_{clientIp}";
        var currentCount = _cache.GetOrCreate(key, entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1);
            return 0;
        });
        
        if (currentCount >= limitPerMinute) return false;
        
        _cache.Set(key, currentCount + 1, TimeSpan.FromMinutes(1));
        return true;
    }
}
```

### **Error Handling**
```csharp
public static class ProblemDetailsMiddleware
{
    public static async Task WriteProblemDetails(HttpContext context, int statusCode, string title, string? detail = null)
    {
        var problemDetails = new ProblemDetails
        {
            Type = $"https://tools.ietf.org/html/rfc7231#section-6.5.{statusCode}",
            Title = title,
            Status = statusCode,
            Detail = detail,
            CorrelationId = context.Request.Headers["X-Correlation-ID"].FirstOrDefault()
        };
        
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";
        await context.Response.WriteAsJsonAsync(problemDetails);
    }
}
```

## 📊 Performance Considerations

### **Database Optimization**
- **Indexed Queries** - Strategic MongoDB indexes
- **Pagination** - Efficient large dataset handling
- **Projection** - Selective field retrieval
- **Aggregation** - Complex query optimization

### **Caching Strategy**
- **Memory Cache** - Rate limiting and session data
- **Response Caching** - Future implementation opportunity
- **Database Query Caching** - MongoDB query result caching

### **Async/Await Pattern**
```csharp
// Parallel execution for better performance
var totalTask = _collection.CountDocumentsAsync(filter, cancellationToken: cancellationToken);
var itemsTask = find.Skip(skip).Limit(query.PageSize).ToListAsync(cancellationToken);
await Task.WhenAll(totalTask, itemsTask);
```

## 🚀 Scalability Features

### **Horizontal Scaling**
- **Stateless Design** - No server-side state dependencies
- **MongoDB Sharding** - Database horizontal scaling support
- **Load Balancing** - Multiple API instances support

### **Vertical Scaling**
- **Async Operations** - Non-blocking I/O operations
- **Memory Management** - Efficient resource utilization
- **Connection Pooling** - MongoDB connection optimization

## 🔄 Deployment Architecture

### **Container Support**
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 5000

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["Million.Web/Million.Web.csproj", "Million.Web/"]
RUN dotnet restore "Million.Web/Million.Web.csproj"
COPY . .
WORKDIR "/src/Million.Web"
RUN dotnet build "Million.Web.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Million.Web.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Million.Web.dll"]
```

### **Environment Configuration**
- **Development** - Local MongoDB, development settings
- **Staging** - Cloud MongoDB, staging configuration
- **Production** - Production MongoDB, secure settings

## 📈 Monitoring & Observability

### **Logging Strategy**
```csharp
// Serilog configuration
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .Enrich.FromLogContext()
    .Enrich.WithEnvironmentName()
    .WriteTo.Console()
    .CreateLogger();

// Request logging middleware
app.UseSerilogRequestLogging();
```

### **Health Checks**
```csharp
app.MapGet("/health/live", () => Results.Ok(new { status = "ok" }));
app.MapGet("/health/ready", () => Results.Ok(new { status = "ready" }));
```

### **Correlation IDs**
```csharp
public class CorrelationIdMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var correlationId = context.Request.Headers["X-Correlation-ID"].FirstOrDefault() ?? Guid.NewGuid().ToString();
        context.Request.Headers["X-Correlation-ID"] = correlationId;
        context.Response.Headers["X-Correlation-ID"] = correlationId;
        
        await next(context);
    }
}
```

## 🔮 Future Enhancements

### **Planned Features**
- **Authentication & Authorization** - JWT-based security
- **Audit Logging** - Comprehensive change tracking
- **Real-time Updates** - SignalR integration
- **Advanced Analytics** - Property performance metrics
- **Multi-tenancy** - Organization-based data isolation

### **Technical Improvements**
- **Redis Caching** - Distributed caching layer
- **Message Queues** - Asynchronous processing
- **API Versioning** - Backward compatibility
- **GraphQL** - Flexible data querying
- **Microservices** - Service decomposition

---

**This architecture provides a solid foundation for building scalable, maintainable, and feature-rich real estate management systems.**