# Million Real Estate API 🏠

A high-performance .NET 9 real estate API with MongoDB, featuring an aggregate root design, advanced media management, and JWT authentication.

## ✨ Features

### 🏗️ **Aggregate Root Architecture**
- **Property Aggregate**: Central entity managing media, traces, and business logic
- **Media Library**: Featured images (≤12) + paginated library (≤60 total)
- **Future-Ready**: Video support planned with provider integration
- **Legacy Support**: Backward compatibility with existing image fields

### 🔐 **Authentication & Authorization**
- **JWT Tokens**: RS256 signing with configurable TTL
- **Role-Based Access**: Owner and Admin roles with granular permissions
- **Session Management**: Refresh token rotation with TTL cleanup
- **Security**: Argon2id password hashing, account lockout protection

### 📸 **Advanced Media System**
- **Featured Gallery**: Up to 12 high-quality images for public display
- **Media Library**: Paginated access to all media with filtering
- **Vercel Blob Integration**: Direct upload with path validation
- **Smart Indexing**: Unique indices per media type with gap detection

### 🚀 **Performance & Optimization**
- **MongoDB Aggregation**: Efficient pipeline for complex queries
- **Smart Indexing**: Text search, price ranges, media counts
- **Explain Helper**: Query optimization insights and execution plans
- **Rate Limiting**: Per-IP/endpoint with burst support

## 🛠️ Technology Stack

- **.NET 9**: Latest framework with minimal APIs
- **MongoDB**: Document database with aggregation pipelines
- **JWT**: RS256 authentication with refresh tokens
- **Argon2id**: State-of-the-art password hashing
- **Serilog**: Structured logging with correlation IDs
- **FluentValidation**: Comprehensive input validation
- **Swagger/OpenAPI**: Interactive API documentation

## 🚀 Quick Start

### Prerequisites
- .NET 9 SDK
- MongoDB 6.0+
- Docker (optional)

### Environment Variables
```bash
# MongoDB
MONGO_URI=mongodb://localhost:27017
MONGO_DB=million

# JWT Authentication
AUTH_JWT_ISSUER=your-domain.com
AUTH_JWT_AUDIENCE=your-app
AUTH_JWT_PRIVATE_KEY=-----BEGIN PRIVATE KEY-----
AUTH_JWT_PUBLIC_KEY=-----BEGIN PUBLIC KEY-----
AUTH_ACCESS_TTL_MIN=10
AUTH_REFRESH_TTL_DAYS=14
AUTH_LOCKOUT_ATTEMPTS=5
AUTH_LOCKOUT_WINDOW_MIN=15

# Media Limits
FEATURED_MEDIA_LIMIT=12
MEDIA_LIBRARY_LIMIT=60
MAX_UPLOAD_MB=25
ENABLE_VIDEO=false

# Rate Limiting
RATE_LIMIT_PERMINUTE=120
RATE_LIMIT_BURST=200

# CORS & Logging
CORS_ORIGINS=http://localhost:3000
LOG_LEVEL=Information
```

### Generate JWT Keys
```bash
# Generate private key
openssl genrsa -out private.pem 2048

# Generate public key
openssl rsa -in private.pem -pubout -out public.pem

# Convert to base64 for environment variables
cat private.pem | base64 -w 0
cat public.pem | base64 -w 0
```

### Run the API
```bash
# Start MongoDB
docker-compose up -d

# Build and run
dotnet build
dotnet run --project src/Million.Web

# Run tests
dotnet test
```

## 📚 API Endpoints

### 🔐 Authentication
```
POST /auth/owner/login          # Owner login
POST /auth/owner/refresh        # Refresh tokens
POST /auth/owner/logout         # Revoke session
```

### 🏠 Properties
```
GET    /properties              # List with filters & pagination
GET    /properties/{id}         # Property details
POST   /properties              # Create property
PUT    /properties/{id}         # Update property
DELETE /properties/{id}         # Delete property
PATCH  /properties/{id}/activate    # Activate property
PATCH  /properties/{id}/deactivate  # Deactivate property
```

### 📸 Media Management
```
GET    /properties/{id}/media   # Paginated media library
PATCH  /properties/{id}/media   # Update media metadata
POST   /properties/{id}/traces  # Add sales history
GET    /properties/explain      # Query optimization insights
```

### 👑 Admin Operations
```
GET    /admin/owners            # List all owners
GET    /admin/owners/{id}       # Owner details
GET    /admin/owners/{id}/sessions  # Owner sessions
POST   /admin/owners            # Create owner
PATCH  /admin/owners/{id}       # Update owner
POST   /admin/owners/{id}/revoke-session/{sid}  # Revoke session
```

## 🗄️ Data Model

### Property Aggregate Root
```json
{
  "_id": "string",
  "ownerId": "string",
  "name": "string",
  "address": "string",
  "price": "decimal",
  "codeInternal": "string",
  "year": "int",
  "status": "active|sold|offmarket",
  
  "cover": {
    "type": "image|video",
    "url": "string",
    "poster": "string?"
  },
  
  "media": [
    {
      "type": "image|video",
      "url": "string",
      "index": "int",
      "enabled": "bool",
      "featured": "bool",
      "variants": {
        "small": "string?",
        "medium": "string?",
        "large": "string?"
      }
    }
  ],
  
  "traces": [
    {
      "dateSale": "date",
      "name": "string",
      "value": "decimal",
      "tax": "decimal"
    }
  ]
}
```

### Owner Entity
```json
{
  "_id": "string",
  "fullName": "string",
  "email": "string",
  "phoneE164": "string?",
  "photoUrl": "string?",
  "role": "owner|admin",
  "isActive": "bool",
  "passwordHash": "string"
}
```

## 🔍 Query Examples

### Advanced Property Search
```bash
GET /properties?search=luxury&location=malibu&minPrice=1000000&maxPrice=5000000&bedrooms=4&hasPool=true&sort=-price&page=1&pageSize=20
```

### Media Library Access
```bash
GET /properties/mlb001/media?type=image&featured=false&page=1&pageSize=20
```

### Query Optimization
```bash
GET /properties/explain?search=luxury&location=malibu&minPrice=1000000
```

## 📸 Vercel Blob Integration

### Upload Flow
1. **Client**: Upload directly to Vercel Blob using `/api/blob/upload`
2. **Path Structure**: `properties/{id}/cover.ext` and `properties/{id}/{index}.ext`
3. **Validation**: HTTPS URLs matching Blob path patterns
4. **Storage**: Only metadata stored in MongoDB

### Path Validation
```
properties/{id}/cover.(jpg|jpeg|png|webp|avif)
properties/{id}/[1-9][0-9]*.(jpg|jpeg|png|webp|avif)
```

## 🔒 Security Features

### Password Security
- **Argon2id**: Memory-hard hashing with configurable parameters
- **Salt Management**: Unique salt per password
- **Iterations**: 3 iterations with 64MB memory usage

### JWT Security
- **RS256**: Asymmetric signing for enhanced security
- **Token Rotation**: Refresh tokens rotated on each use
- **TTL Management**: Configurable access and refresh token lifetimes

### Rate Limiting
- **Per-IP**: Individual client rate limiting
- **Burst Support**: Configurable burst allowances
- **Endpoint-Specific**: Different limits per endpoint type

## 🧪 Testing

### Test Coverage
- **Unit Tests**: NUnit with NSubstitute mocking
- **Coverage Target**: ≥80% Application layer coverage
- **Test Categories**: Validators, Services, Controllers, Middleware

### Run Tests
```bash
# All tests
dotnet test

# Specific test category
dotnet test --filter "Category=Validators"

# With coverage
dotnet test --collect:"XPlat Code Coverage"
```

## 📊 Performance Monitoring

### Health Checks
```
GET /health/live    # Liveness probe
GET /health/ready   # Readiness probe
```

### Query Optimization
- **Explain Helper**: Detailed MongoDB execution plans
- **Index Monitoring**: Performance metrics for all indexes
- **Aggregation Pipeline**: Optimized for complex queries

## 🚀 Deployment

### Docker Support
```bash
# Build image
docker build -t million-api .

# Run container
docker run -p 5000:5000 million-api
```

### Environment Configuration
- **Development**: Local MongoDB, debug logging
- **Production**: Production MongoDB, structured logging
- **Staging**: Staging environment with test data

## 🔄 Migration

### Legacy Image Migration
```bash
# Run migration command
dotnet run --project src/Million.Web -- migrate-legacy-images
```

### Migration Process
1. **Legacy Fields**: `coverImage` → `cover.url`
2. **Gallery Images**: `images[]` → `media[]` with featured flags
3. **Backward Compatibility**: Legacy fields maintained during transition
4. **Data Integrity**: Validation and rollback support

## 📈 Future Enhancements

### Video Support
- **Provider Integration**: Mux, Cloudflare, or custom providers
- **Poster Images**: Required for video media
- **Duration Tracking**: Video length metadata
- **Streaming**: Adaptive bitrate streaming

### Advanced Features
- **Real-time Updates**: WebSocket support for live data
- **Analytics**: Property view and interaction tracking
- **Recommendations**: ML-based property suggestions
- **Multi-language**: Internationalization support

## 🤝 Contributing

1. **Fork** the repository
2. **Create** a feature branch
3. **Implement** with tests
4. **Submit** a pull request

### Code Standards
- **Clean Architecture**: Clear separation of concerns
- **Async/Await**: Consistent asynchronous patterns
- **Validation**: Comprehensive input validation
- **Error Handling**: RFC 7807 Problem Details
- **Logging**: Structured logging with correlation IDs

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🆘 Support

- **Issues**: GitHub Issues for bug reports
- **Discussions**: GitHub Discussions for questions
- **Documentation**: Comprehensive API documentation
- **Examples**: Sample requests and responses

---

**Built with ❤️ using .NET 9, MongoDB, and modern web technologies**

