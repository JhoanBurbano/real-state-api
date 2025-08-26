# Million Properties API - Development Guide 🛠️

## 🚀 Getting Started

### **Prerequisites**
- **.NET 9 SDK** - [Download from Microsoft](https://dotnet.microsoft.com/download/dotnet/9.0)
- **MongoDB** - Local installation or Docker
- **Node.js 18+** - For Next.js image upload service
- **Docker** - For local MongoDB setup (optional)
- **IDE** - Visual Studio 2022, VS Code, or Rider

### **Environment Setup**
```bash
# Verify .NET installation
dotnet --version  # Should show 9.0.x

# Verify Node.js installation
node --version    # Should show 18.x or higher

# Verify Docker installation (if using)
docker --version
```

## 🏗️ Project Structure

```
real-state-api/
├── src/
│   ├── Million.Domain/           # Core business entities
│   ├── Million.Application/       # Business logic and DTOs
│   ├── Million.Infrastructure/   # Data access and external services
│   └── Million.Web/              # API endpoints and middleware
├── tests/
│   └── Million.Tests/            # Unit and integration tests
├── ops/                          # Docker and deployment configs
├── docs/                         # Documentation files
└── app/                          # Next.js image upload service
```

## 🔧 Local Development Setup

### **1. Clone and Setup**
```bash
git clone <repository-url>
cd real-state-api

# Restore dependencies
dotnet restore
```

### **2. Start MongoDB**
```bash
# Option A: Using Docker (Recommended)
cd ops
docker compose up -d mongo

# Option B: Local MongoDB
mongod --dbpath /data/db
```

### **3. Run Backend API**
```bash
cd src/Million.Web
dotnet run --urls "http://localhost:5000"
```

### **4. Run Next.js Image Service**
```bash
cd app
npm install
npm run dev
```

### **5. Seed Database**
```bash
# Using MongoDB shell
mongosh million
db.properties.insertMany([...seed data...])

# Or using mongoimport
mongoimport --db million --collection properties --file ops/db/properties.seed.json
```

## 🧪 Testing

### **Run All Tests**
```bash
# From project root
dotnet test

# With detailed output
dotnet test --logger "console;verbosity=detailed"

# Specific test project
dotnet test tests/Million.Tests/
```

### **Test Categories**
- **Unit Tests** - Individual component testing
- **Integration Tests** - Component interaction testing
- **Validation Tests** - Business rule enforcement
- **Middleware Tests** - Cross-cutting concerns

### **Test Coverage**
```bash
# Install coverlet collector
dotnet tool install --global coverlet.collector

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"
```

## 🔍 API Development

### **Adding New Endpoints**

#### **1. Define DTOs**
```csharp
// In Million.Application/DTOs/
public class NewFeatureRequest
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [Range(0, 100)]
    public int Value { get; set; }
}
```

#### **2. Create Validator**
```csharp
// In Million.Application/Validation/
public class NewFeatureRequestValidator : AbstractValidator<NewFeatureRequest>
{
    public NewFeatureRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(100).WithMessage("Name cannot exceed 100 characters");
            
        RuleFor(x => x.Value)
            .InclusiveBetween(0, 100).WithMessage("Value must be between 0 and 100");
    }
}
```

#### **3. Add Service Method**
```csharp
// In Million.Application/Interfaces/IPropertyService
Task<NewFeatureDto> CreateNewFeatureAsync(NewFeatureRequest request, CancellationToken cancellationToken);

// In Million.Application/Services/PropertyService
public async Task<NewFeatureDto> CreateNewFeatureAsync(NewFeatureRequest request, CancellationToken cancellationToken)
{
    // Implementation
}
```

#### **4. Add Repository Method**
```csharp
// In Million.Application/Interfaces/IPropertyRepository
Task<NewFeature> CreateNewFeatureAsync(NewFeature feature, CancellationToken cancellationToken);

// In Million.Infrastructure/Repositories/PropertyRepository
public async Task<NewFeature> CreateNewFeatureAsync(NewFeature feature, CancellationToken cancellationToken)
{
    // Implementation
}
```

#### **5. Add API Endpoint**
```csharp
// In Million.Web/Program.cs
app.MapPost("/new-feature", async (
    NewFeatureRequest request,
    IValidator<NewFeatureRequest> validator,
    SimpleRateLimiter limiter,
    HttpContext http,
    IPropertyService service,
    CancellationToken ct) =>
{
    // Rate limiting
    var clientIp = http.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    var limitPerMinute = int.TryParse(Environment.GetEnvironmentVariable("RATE_LIMIT_PERMINUTE"), out var c) ? c : 120;
    if (!limiter.Allow(clientIp, limitPerMinute))
    {
        http.Response.Headers["Retry-After"] = "60";
        await ProblemDetailsMiddleware.WriteProblemDetails(http, StatusCodes.Status429TooManyRequests, "Too Many Requests");
        return Results.StatusCode(StatusCodes.Status429TooManyRequests);
    }

    // Validation
    var result = await validator.ValidateAsync(request, ct);
    if (!result.IsValid)
    {
        var detail = string.Join("; ", result.Errors.Select(e => e.ErrorMessage));
        await ProblemDetailsMiddleware.WriteProblemDetails(http, StatusCodes.Status400BadRequest, "Validation Failed", detail);
        return Results.ValidationProblem(result.ToDictionary());
    }

    // Business logic
    var newFeature = await service.CreateNewFeatureAsync(request, ct);
    return Results.Created($"/new-feature/{newFeature.Id}", newFeature);
}).WithTags("New Feature");
```

### **Adding New Filters**

#### **1. Extend PropertyListQuery**
```csharp
public class PropertyListQuery
{
    // Existing properties...
    
    // New filter
    public string? NewFilter { get; set; }
}
```

#### **2. Update Validator**
```csharp
public class PropertyListQueryValidator : AbstractValidator<PropertyListQuery>
{
    public PropertyListQueryValidator()
    {
        // Existing rules...
        
        RuleFor(x => x.NewFilter)
            .MaximumLength(50).WithMessage("NewFilter cannot exceed 50 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.NewFilter));
    }
}
```

#### **3. Update Repository**
```csharp
public async Task<(IReadOnlyList<Property> Items, long Total)> FindAsync(PropertyListQuery query, CancellationToken cancellationToken)
{
    var filters = new List<FilterDefinition<PropertyDocument>>();
    var builder = Builders<PropertyDocument>.Filter;

    // Existing filters...
    
    // New filter
    if (!string.IsNullOrWhiteSpace(query.NewFilter))
    {
        filters.Add(builder.Regex(x => x.NewField, new BsonRegularExpression(query.NewFilter, "i")));
    }
    
    // Rest of implementation...
}
```

## 🗄️ Database Development

### **Adding New Fields**

#### **1. Update Domain Entity**
```csharp
// In Million.Domain/Entities/Property.cs
public class Property
{
    // Existing properties...
    
    public string NewField { get; set; } = string.Empty;
    public DateTime? NewDateField { get; set; }
}
```

#### **2. Update DTOs**
```csharp
// In Million.Application/DTOs/PropertyDto.cs
public class PropertyDto
{
    // Existing properties...
    
    public string NewField { get; set; } = string.Empty;
    public DateTime? NewDateField { get; set; }
}

// In Million.Application/DTOs/CreatePropertyRequest.cs
public class CreatePropertyRequest
{
    // Existing properties...
    
    [StringLength(100)]
    public string NewField { get; set; } = string.Empty;
    
    public DateTime? NewDateField { get; set; }
}
```

#### **3. Update Document Model**
```csharp
// In Million.Infrastructure/Persistence/PropertyDocument.cs
public class PropertyDocument
{
    // Existing properties...
    
    public string NewField { get; set; } = string.Empty;
    public DateTime? NewDateField { get; set; }
    
    public static PropertyDocument FromEntity(Property entity)
    {
        return new PropertyDocument
        {
            // Existing mappings...
            NewField = entity.NewField,
            NewDateField = entity.NewDateField
        };
    }
    
    public Property ToEntity()
    {
        return new Property
        {
            // Existing mappings...
            NewField = NewField,
            NewDateField = NewDateField
        };
    }
}
```

#### **4. Update Service Mapping**
```csharp
// In Million.Application/Services/PropertyService.cs
private static PropertyDto MapToDto(Property entity) => new()
{
    // Existing mappings...
    NewField = entity.NewField,
    NewDateField = entity.NewDateField
};
```

### **Database Migrations**

#### **1. Create Migration Script**
```javascript
// In ops/db/migrations/
// 001-add-new-fields.js
db.properties.updateMany(
    {},
    {
        $set: {
            newField: "",
            newDateField: null
        }
    }
);
```

#### **2. Create Indexes**
```javascript
// In ops/db/create-indexes.js
db.properties.createIndex({ "newField": 1 });
db.properties.createIndex({ "newDateField": 1 });
```

#### **3. Run Migration**
```bash
# Using MongoDB shell
mongosh million --file ops/db/migrations/001-add-new-fields.js

# Or manually
mongosh million
source("ops/db/migrations/001-add-new-fields.js")
```

## 🖼️ Image Management Development

### **Adding New Image Types**

#### **1. Update Validation Regex**
```csharp
// In validators
private static readonly Regex BlobUrlRegex = new(
    @"^https://[a-z0-9.-]+\.public\.blob\.vercel-storage\.com/properties/([a-zA-Z0-9]+)/(cover|floorplan|([1-9]|1[0-2]))\.[a-zA-Z0-9]+$",
    RegexOptions.Compiled | RegexOptions.IgnoreCase);
```

#### **2. Update Next.js API Route**
```typescript
// In app/api/blob/upload/route.ts
export async function POST(request: NextRequest) {
  const { propertyId, kind, index } = await request.json();

  // Validation
  if (!['cover', 'floorplan', 'gallery'].includes(kind)) {
    return NextResponse.json(
      { error: 'kind must be either "cover", "floorplan", or "gallery"' },
      { status: 400 }
    );
  }

  // Compute pathname
  let pathname: string;
  if (kind === 'cover') {
    pathname = `properties/${propertyId}/cover`;
  } else if (kind === 'floorplan') {
    pathname = `properties/${propertyId}/floorplan`;
  } else {
    pathname = `properties/${propertyId}/${index}`;
  }

  // Rest of implementation...
}
```

#### **3. Update Client Helper**
```typescript
// In app/utils/blob.ts
export interface BlobUploadOptions {
  propertyId: string;
  kind: 'cover' | 'floorplan' | 'gallery';
  index?: number;
  file: File;
}

export async function uploadFloorplan(propertyId: string, file: File): Promise<BlobUploadResponse> {
  return uploadPropertyImage({
    propertyId,
    kind: 'floorplan',
    file,
  });
}
```

## 🔒 Security Development

### **Adding New Validation Rules**

#### **1. Custom Validators**
```csharp
// In Million.Application/Validation/
public static class CustomValidators
{
    public static IRuleBuilderOptions<T, string> MustBeValidPhoneNumber<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder.Must(phoneNumber =>
        {
            if (string.IsNullOrEmpty(phoneNumber)) return false;
            return Regex.IsMatch(phoneNumber, @"^\+?[1-9]\d{1,14}$");
        }).WithMessage("Phone number must be a valid international format");
    }
    
    public static IRuleBuilderOptions<T, string> MustBeValidEmail<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder.Must(email =>
        {
            if (string.IsNullOrEmpty(email)) return false;
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }).WithMessage("Email must be a valid email address");
    }
}
```

#### **2. Using Custom Validators**
```csharp
public class CreatePropertyRequestValidator : AbstractValidator<CreatePropertyRequest>
{
    public CreatePropertyRequestValidator()
    {
        // Existing rules...
        
        RuleFor(x => x.PhoneNumber)
            .MustBeValidPhoneNumber();
            
        RuleFor(x => x.Email)
            .MustBeValidEmail();
    }
}
```

### **Rate Limiting Customization**

#### **1. Custom Rate Limiter**
```csharp
// In Million.Web/Middlewares/
public class AdvancedRateLimiter
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<AdvancedRateLimiter> _logger;

    public AdvancedRateLimiter(IMemoryCache cache, ILogger<AdvancedRateLimiter> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public bool Allow(string clientIp, string endpoint, int limitPerMinute)
    {
        var key = $"rate_limit_{clientIp}_{endpoint}";
        var currentCount = _cache.GetOrCreate(key, entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1);
            return 0;
        });

        if (currentCount >= limitPerMinute)
        {
            _logger.LogWarning("Rate limit exceeded for {ClientIp} on {Endpoint}", clientIp, endpoint);
            return false;
        }

        _cache.Set(key, currentCount + 1, TimeSpan.FromMinutes(1));
        return true;
    }
}
```

#### **2. Using Advanced Rate Limiter**
```csharp
// In Program.cs
builder.Services.AddSingleton<AdvancedRateLimiter>();

// In endpoints
app.MapPost("/properties", async (
    CreatePropertyRequest request,
    IValidator<CreatePropertyRequest> validator,
    AdvancedRateLimiter limiter,
    HttpContext http,
    IPropertyService service,
    CancellationToken ct) =>
{
    var clientIp = http.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    var limitPerMinute = int.TryParse(Environment.GetEnvironmentVariable("RATE_LIMIT_PERMINUTE"), out var c) ? c : 120;
    
    if (!limiter.Allow(clientIp, "create_property", limitPerMinute))
    {
        http.Response.Headers["Retry-After"] = "60";
        await ProblemDetailsMiddleware.WriteProblemDetails(http, StatusCodes.Status429TooManyRequests, "Too Many Requests");
        return Results.StatusCode(StatusCodes.Status429TooManyRequests);
    }

    // Rest of implementation...
});
```

## 🧪 Testing Development

### **Adding New Tests**

#### **1. Test Structure**
```csharp
[TestFixture]
public class NewFeatureTests
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
    public async Task NewFeature_WithValidInput_ReturnsSuccess()
    {
        // Arrange
        var request = new NewFeatureRequest { Name = "Test", Value = 50 };

        // Act
        var result = await _mockService.CreateNewFeatureAsync(request, CancellationToken.None);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Name, Is.EqualTo("Test"));
    }

    [Test]
    public async Task NewFeature_WithInvalidInput_ThrowsException()
    {
        // Arrange
        var request = new NewFeatureRequest { Name = "", Value = 150 };

        // Act & Assert
        var ex = Assert.ThrowsAsync<ValidationException>(async () =>
            await _mockService.CreateNewFeatureAsync(request, CancellationToken.None));
        
        Assert.That(ex.Message, Contains.Substring("Name is required"));
    }
}
```

#### **2. Integration Tests**
```csharp
[TestFixture]
public class NewFeatureIntegrationTests
{
    private WebApplicationFactory<Program> _factory;
    private HttpClient _client;

    [SetUp]
    public void Setup()
    {
        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Replace real services with test doubles
                    services.Replace(ServiceDescriptor.Singleton<IPropertyService, MockPropertyService>());
                });
            });
        _client = _factory.CreateClient();
    }

    [Test]
    public async Task NewFeature_EndToEnd_ReturnsSuccess()
    {
        // Arrange
        var request = new { Name = "Integration Test", Value = 75 };

        // Act
        var response = await _client.PostAsJsonAsync("/new-feature", request);

        // Assert
        Assert.That(response.IsSuccessStatusCode, Is.True);
        var result = await response.Content.ReadFromJsonAsync<NewFeatureDto>();
        Assert.That(result.Name, Is.EqualTo("Integration Test"));
    }
}
```

## 📊 Performance Development

### **Adding Caching**

#### **1. Response Caching**
```csharp
// In Program.cs
builder.Services.AddResponseCaching();

// In endpoints
app.MapGet("/properties", async (PropertyListQuery query, ...) =>
{
    // Implementation
}).CacheOutput(policy => policy
    .Expire(TimeSpan.FromMinutes(5))
    .SetVaryByQuery("page", "pageSize", "sort")
    .SetVaryByHeader("Accept-Language"));
```

#### **2. Memory Caching**
```csharp
// In services
public class PropertyService : IPropertyService
{
    private readonly IMemoryCache _cache;
    
    public async Task<PagedResult<PropertyDto>> GetPropertiesAsync(PropertyListQuery query, CancellationToken cancellationToken)
    {
        var cacheKey = $"properties_{JsonSerializer.Serialize(query)}";
        
        if (_cache.TryGetValue(cacheKey, out PagedResult<PropertyDto> cachedResult))
        {
            return cachedResult;
        }

        var result = await _repository.FindAsync(query, cancellationToken);
        var dtoResult = new PagedResult<PropertyDto> { /* mapping */ };
        
        _cache.Set(cacheKey, dtoResult, TimeSpan.FromMinutes(10));
        return dtoResult;
    }
}
```

### **Database Optimization**

#### **1. Query Optimization**
```csharp
public async Task<(IReadOnlyList<Property> Items, long Total)> FindAsync(PropertyListQuery query, CancellationToken cancellationToken)
{
    // Use projection to select only needed fields
    var projection = Builders<PropertyDocument>.Projection
        .Include(x => x.Id)
        .Include(x => x.Name)
        .Include(x => x.PriceProperty)
        .Include(x => x.City)
        .Include(x => x.Bedrooms)
        .Include(x => x.Image);

    var find = _collection.Find(filter).Project<PropertyDocument>(projection);
    
    // Rest of implementation...
}
```

#### **2. Aggregation Pipeline**
```csharp
public async Task<PropertyStats> GetPropertyStatsAsync(CancellationToken cancellationToken)
{
    var pipeline = new[]
    {
        new BsonDocument("$match", new BsonDocument("isActive", true)),
        new BsonDocument("$group", new BsonDocument
        {
            { "_id", BsonNull.Value },
            { "totalProperties", new BsonDocument("$sum", 1) },
            { "avgPrice", new BsonDocument("$avg", "$priceProperty") },
            { "minPrice", new BsonDocument("$min", "$priceProperty") },
            { "maxPrice", new BsonDocument("$max", "$priceProperty") }
        })
    };

    var result = await _collection.Aggregate<BsonDocument>(pipeline).FirstOrDefaultAsync(cancellationToken);
    return MapToPropertyStats(result);
}
```

## 🚀 Deployment Development

### **Environment Configuration**

#### **1. Environment-Specific Settings**
```json
// appsettings.Development.json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Information"
    }
  },
  "Mongo": {
    "Uri": "mongodb://localhost:27017",
    "Database": "million_dev"
  },
  "RateLimit": {
    "PerMinute": 200
  }
}

// appsettings.Production.json
{
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "Mongo": {
    "Uri": "mongodb://production-server:27017",
    "Database": "million_prod"
  },
  "RateLimit": {
    "PerMinute": 100
  }
}
```

#### **2. Environment Variables**
```bash
# Development
export ASPNETCORE_ENVIRONMENT=Development
export MONGO_URI=mongodb://localhost:27017
export MONGO_DB=million_dev
export RATE_LIMIT_PERMINUTE=200

# Production
export ASPNETCORE_ENVIRONMENT=Production
export MONGO_URI=mongodb://production-server:27017
export MONGO_DB=million_prod
export RATE_LIMIT_PERMINUTE=100
```

### **Docker Development**

#### **1. Multi-Stage Dockerfile**
```dockerfile
# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["Million.Web/Million.Web.csproj", "Million.Web/"]
COPY ["Million.Application/Million.Application.csproj", "Million.Application/"]
COPY ["Million.Infrastructure/Million.Infrastructure.csproj", "Million.Infrastructure/"]
COPY ["Million.Domain/Million.Domain.csproj", "Million.Domain/"]
RUN dotnet restore "Million.Web/Million.Web.csproj"
COPY . .
WORKDIR "/src/Million.Web"
RUN dotnet build "Million.Web.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "Million.Web.csproj" -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
EXPOSE 5000
ENTRYPOINT ["dotnet", "Million.Web.dll"]
```

#### **2. Docker Compose for Development**
```yaml
# docker-compose.dev.yml
version: '3.8'
services:
  mongo:
    image: mongo:7.0
    ports:
      - "27017:27017"
    environment:
      MONGO_INITDB_DATABASE: million_dev
    volumes:
      - mongo_dev_data:/data/db

  mongo-express:
    image: mongo-express:latest
    ports:
      - "8081:8081"
    environment:
      ME_CONFIG_MONGODB_SERVER: mongo
      ME_CONFIG_MONGODB_PORT: 27017
      ME_CONFIG_MONGODB_ENABLE_ADMIN: true
      ME_CONFIG_BASICAUTH_USERNAME: admin
      ME_CONFIG_BASICAUTH_PASSWORD: pass

  api:
    build:
      context: .
      dockerfile: Dockerfile
    ports:
      - "5000:5000"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - MONGO_URI=mongodb://mongo:27017
      - MONGO_DB=million_dev
    depends_on:
      - mongo

volumes:
  mongo_dev_data:
```

## 🔍 Debugging and Troubleshooting

### **Common Issues**

#### **1. MongoDB Connection Issues**
```bash
# Check MongoDB status
docker compose ps mongo

# Check MongoDB logs
docker compose logs mongo

# Test connection
mongosh "mongodb://localhost:27017/million"
```

#### **2. Validation Errors**
```bash
# Check validation rules
dotnet build --verbosity detailed

# Run specific validation tests
dotnet test --filter "FullyQualifiedName~PropertyCreateValidatorTests"
```

#### **3. Rate Limiting Issues**
```bash
# Check rate limit configuration
echo $RATE_LIMIT_PERMINUTE

# Monitor requests
curl -H "X-Correlation-ID: test123" http://localhost:5000/health/live
```

### **Logging and Monitoring**

#### **1. Serilog Configuration**
```csharp
// In Program.cs
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .Enrich.FromLogContext()
    .Enrich.WithEnvironmentName()
    .Enrich.WithProperty("Application", "Million Properties API")
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
    .WriteTo.File("logs/api-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();
```

#### **2. Request Logging**
```csharp
// In Program.cs
app.UseSerilogRequestLogging(options =>
{
    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
    {
        diagnosticContext.Set("ClientIP", httpContext.Connection.RemoteIpAddress?.ToString());
        diagnosticContext.Set("UserAgent", httpContext.Request.Headers["User-Agent"].FirstOrDefault());
        diagnosticContext.Set("CorrelationId", httpContext.Request.Headers["X-Correlation-ID"].FirstOrDefault());
    };
});
```

## 📚 Best Practices

### **Code Organization**
- **Single Responsibility** - Each class has one reason to change
- **Dependency Inversion** - Depend on abstractions, not concretions
- **Interface Segregation** - Keep interfaces focused and cohesive
- **Open/Closed Principle** - Open for extension, closed for modification

### **Error Handling**
- **Use ProblemDetails** - RFC 7807 compliant error responses
- **Include Correlation IDs** - For request tracing
- **Log Exceptions** - With context and correlation IDs
- **Graceful Degradation** - Handle errors without crashing

### **Performance**
- **Async/Await** - Use throughout the application
- **Database Indexes** - Strategic indexing for common queries
- **Caching** - Cache frequently accessed data
- **Pagination** - Limit result sets for large datasets

### **Security**
- **Input Validation** - Validate all inputs
- **Rate Limiting** - Prevent abuse
- **CORS Configuration** - Restrict cross-origin access
- **Secure Headers** - Use security headers

---

**This development guide provides comprehensive information for extending and maintaining the Million Properties API.**