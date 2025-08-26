# 🗺️ **API Routes Mapping - Million Real Estate API**

## 📍 **Base URL**
```
https://million-real-estate-api-sh25jnp3aa-uc.a.run.app
```

## 🔐 **Authentication Endpoints**

### **POST** `/auth/login`
- **Description**: Authenticate owner and get JWT tokens
- **Request Body**: `LoginRequest`
- **Response**: `LoginResponse` (JWT + Refresh Token)
- **Status Codes**: 200, 400, 401

### **POST** `/auth/refresh`
- **Description**: Refresh JWT token using refresh token
- **Request Body**: `RefreshRequest`
- **Response**: `LoginResponse` (New JWT + Refresh Token)
- **Status Codes**: 200, 400, 401

### **POST** `/auth/logout`
- **Description**: Logout and invalidate refresh token
- **Headers**: `Authorization: Bearer {JWT}`
- **Status Codes**: 200, 401

## 🏠 **Properties Endpoints**

### **GET** `/properties`
- **Description**: Get paginated list of properties with filters
- **Query Parameters**:
  - `page` (int): Page number (default: 1)
  - `pageSize` (int): Items per page (default: 10, max: 100)
  - `search` (string): Search in name, description, address
  - `city` (string): Filter by city
  - `neighborhood` (string): Filter by neighborhood
  - `propertyType` (string): Filter by property type
  - `minPrice` (decimal): Minimum price filter
  - `maxPrice` (decimal): Maximum price filter
  - `bedrooms` (int): Filter by number of bedrooms
  - `bathrooms` (int): Filter by number of bathrooms
  - `minSize` (decimal): Minimum size filter
  - `maxSize` (decimal): Maximum size filter
  - `hasPool` (bool): Filter by pool availability
  - `hasGarden` (bool): Filter by garden availability
  - `hasParking` (bool): Filter by parking availability
  - `isFurnished` (bool): Filter by furnishing status
  - `availableFrom` (datetime): Available from date
  - `availableTo` (datetime): Available to date
  - `sort` (string): Sort field (name, price, year, createdAt)
- **Response**: `PagedResult<PropertyListDto>`
- **Status Codes**: 200, 400, 401

### **GET** `/properties/{id}`
- **Description**: Get property details by ID
- **Path Parameters**: `id` (string): Property ID
- **Response**: `PropertyDetailDto`
- **Status Codes**: 200, 404, 401

### **POST** `/properties`
- **Description**: Create new property
- **Headers**: `Authorization: Bearer {JWT}`
- **Request Body**: `CreatePropertyRequest`
- **Response**: `PropertyDetailDto`
- **Status Codes**: 201, 400, 401, 403

### **PUT** `/properties/{id}`
- **Description**: Update existing property
- **Headers**: `Authorization: Bearer {JWT}`
- **Path Parameters**: `id` (string): Property ID
- **Request Body**: `UpdatePropertyRequest`
- **Response**: `PropertyDetailDto`
- **Status Codes**: 200, 400, 401, 403, 404

### **DELETE** `/properties/{id}`
- **Description**: Delete property (soft delete)
- **Headers**: `Authorization: Bearer {JWT}`
- **Path Parameters**: `id` (string): Property ID
- **Status Codes**: 204, 401, 403, 404

### **PATCH** `/properties/{id}/media`
- **Description**: Update property media (cover, gallery)
- **Headers**: `Authorization: Bearer {JWT}`
- **Path Parameters**: `id` (string): Property ID
- **Request Body**: `MediaPatchDto`
- **Response**: `PropertyDetailDto`
- **Status Codes**: 200, 400, 401, 403, 404

## 👥 **Owners Endpoints**

### **GET** `/owners/profile`
- **Description**: Get current owner profile
- **Headers**: `Authorization: Bearer {JWT}`
- **Response**: `OwnerDto` with full profile information
- **Status Codes**: 200, 401
- **Features**: 
  - Returns complete owner profile
  - Includes role and account status
  - Requires valid JWT authentication
- **Response Fields**: `id`, `fullName`, `email`, `phoneE164`, `photoUrl`, `role`, `isActive`, `createdAt`, `updatedAt`

### **PUT** `/owners/profile`
- **Description**: Update owner profile
- **Headers**: `Authorization: Bearer {JWT}`
- **Request Body**: `UpdateOwnerRequest`
- **Response**: `OwnerDto`
- **Status Codes**: 200, 400, 401
- **Features**: 
  - Partial updates supported
  - Validates field constraints
  - Updates timestamp automatically
- **Updateable Fields**: `fullName` (max 200 chars), `phoneE164` (max 20 chars), `photoUrl` (max 500 chars)

### **POST** `/owners`
- **Description**: Create new owner account
- **Request Body**: `CreateOwnerRequest`
- **Response**: `OwnerDto`
- **Status Codes**: 201, 400

## 🖼️ **Media Endpoints**

### **GET** `/properties/{id}/media`
- **Description**: Get property media gallery
- **Path Parameters**: `id` (string): Property ID
- **Query Parameters**:
  - `type` (string): Media type filter (Image, Video)
  - `featured` (bool): Featured media only
  - `page` (int): Page number
  - `pageSize` (int): Items per page
- **Response**: `List<MediaDto>`
- **Status Codes**: 200, 404

## 📊 **Analytics Endpoints**

### **GET** `/properties/{id}/traces`
- **Description**: Get property transaction history
- **Path Parameters**: `id` (string): Property ID
- **Response**: `List<PropertyTraceDto>`
- **Status Codes**: 200, 404

## 🏥 **Health Check Endpoints**

### **GET** `/health/live`
- **Description**: Liveness probe for container health
- **Response**: `{ "status": "Healthy" }`
- **Status Codes**: 200

### **GET** `/health/ready`
- **Description**: Readiness probe for service readiness
- **Response**: `{ "status": "Healthy" }`
- **Status Codes**: 200

## 🔍 **Search & Filter Endpoints**

### **POST** `/properties/search`
- **Description**: Advanced property search with full-text search
- **Request Body**: `AdvancedSearchRequest`
- **Response**: `PagedResult<PropertyListDto>`
- **Status Codes**: 200, 400

## 📈 **Statistics Endpoints**

### **GET** `/stats/properties`
- **Description**: Get property statistics
- **Query Parameters**:
  - `city` (string): Filter by city
  - `propertyType` (string): Filter by property type
- **Response**: `PropertyStatsDto`
- **Status Codes**: 200

## 🚨 **Error Response Format**

All error responses follow RFC 7807 Problem Details format:

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Bad Request",
  "status": 400,
  "detail": "Validation failed",
  "instance": "/properties",
  "timestamp": "2024-01-01T00:00:00Z",
  "extensions": {
    "correlationId": "uuid",
    "requestId": "request-id",
    "endpoint": "/properties",
    "method": "POST",
    "clientIp": "127.0.0.1"
  }
}
```

## 📝 **Common Query Parameters**

### **Pagination**
- `page`: Page number (1-based)
- `pageSize`: Items per page (1-100)

### **Sorting**
- `sort`: Field to sort by
  - `name`: Property name
  - `price`: Property price
  - `year`: Construction year
  - `createdAt`: Creation date
  - `-field`: Descending order

### **Filtering**
- `status`: Property status (Active, Sold, OffMarket)
- `isActive`: Active properties only (true/false)

## 🔐 **Authentication Headers**

```http
Authorization: Bearer {JWT_TOKEN}
```

## 📊 **Rate Limiting**

- **Limit**: 100 requests per minute per IP
- **Headers**:
  - `X-RateLimit-Limit`: Request limit
  - `X-RateLimit-Remaining`: Remaining requests
  - `X-RateLimit-Reset`: Reset time (Unix timestamp)

## 🌐 **CORS Headers**

```http
Access-Control-Allow-Origin: *
Access-Control-Allow-Methods: GET, POST, PUT, DELETE, PATCH, OPTIONS
Access-Control-Allow-Headers: Content-Type, Authorization
```

## 📱 **Mobile-Friendly Features**

- **Responsive Design**: All endpoints support mobile devices
- **Image Optimization**: Automatic image resizing and compression
- **Progressive Loading**: Pagination and lazy loading support
- **Offline Support**: Caching headers for offline access

## 🔄 **Webhook Support**

### **POST** `/webhooks/property-updated`
- **Description**: Webhook for property updates
- **Headers**: `X-Webhook-Signature`: HMAC signature
- **Request Body**: Property update event data
- **Status Codes**: 200, 400, 401

## 🔍 **Advanced Search Endpoints**

### **POST** `/properties/search`
- **Description**: Advanced property search with complex filters
- **Authentication**: Public endpoint
- **Request Body**: `AdvancedSearchRequest`
- **Response**: `PagedResult<PropertyListDto>`
- **Status Codes**: 200, 400

**Request Body Example:**
```json
{
  "query": "villa miami beach",
  "filters": {
    "city": "Miami Beach",
    "propertyType": "Villa",
    "priceRange": {
      "min": 1000000,
      "max": 5000000
    },
    "sizeRange": {
      "min": 200,
      "max": 500
    },
    "rooms": {
      "minBedrooms": 3,
      "minBathrooms": 2
    },
    "amenities": {
      "hasPool": true,
      "hasGarden": true,
      "hasParking": true,
      "isFurnished": false
    },
    "availability": {
      "from": "2024-01-01",
      "to": "2024-12-31"
    },
    "status": "Active"
  },
  "sort": {
    "field": "price",
    "order": "desc"
  },
  "pagination": {
    "page": 1,
    "pageSize": 20
  }
}
```

## 📊 **Property Timeline & Traces**

### **GET** `/properties/{id}/traces`
- **Description**: Get property transaction history and timeline
- **Authentication**: Public endpoint (no authentication required)
- **Path Parameters**: `id` - Property ID
- **Response**: `List<PropertyTraceDto>`
- **Status Codes**: 200, 404

**Response Example:**
```json
[
  {
    "id": "trace-001",
    "propertyId": "prop-001",
    "action": "Created",
    "previousValue": null,
    "newValue": "Luxury Villa Miami Beach",
    "timestamp": "2024-01-01T00:00:00Z",
    "userId": "owner-001",
    "notes": "Property created",
    "propertyName": "Luxury Villa Miami Beach",
    "price": 2500000,
    "status": "Active",
    "formattedTimestamp": "Jan 01, 2024 00:00",
    "actionDisplayName": "Property Created",
    "changeDescription": "Property created: Luxury Villa Miami Beach"
  }
]
```

## 🖼️ **Media Management Endpoints**

### **PATCH** `/properties/{id}/media`
- **Description**: Update property media (cover + gallery) independently
- **Authentication**: JWT required
- **Headers**: `Authorization: Bearer {JWT}`
- **Path Parameters**: `id` - Property ID
- **Request Body**: `MediaPatchDto`
- **Response**: `PropertyDetailDto`
- **Status Codes**: 200, 400, 401, 404

**Request Body Example:**
```json
{
  "cover": {
    "url": "https://blob.vercel-storage.com/properties/prop-001/cover.jpg",
    "type": "image",
    "index": 0
  },
  "gallery": [
    {
      "id": "media-001",
      "url": "https://blob.vercel-storage.com/properties/prop-001/gallery-1.jpg",
      "type": "image",
      "index": 1,
      "enabled": true,
      "featured": true
    }
  ],
  "notes": "Updated property media"
}
```

### **PATCH** `/properties/{id}/media/reorder`
- **Description**: Reorder gallery items
- **Authentication**: JWT required
- **Headers**: `Authorization: Bearer {JWT}`
- **Path Parameters**: `id` - Property ID
- **Request Body**: `List<string>` - Media IDs in new order
- **Response**: `bool`
- **Status Codes**: 200, 400, 401, 404

### **PATCH** `/properties/{id}/media/{mediaId}/feature`
- **Description**: Set/unset featured media
- **Authentication**: JWT required
- **Headers**: `Authorization: Bearer {JWT}`
- **Path Parameters**: 
  - `id` - Property ID
  - `mediaId` - Media ID
- **Request Body**: `{ "featured": true }`
- **Response**: `bool`
- **Status Codes**: 200, 400, 401, 404

### **PATCH** `/properties/{id}/media/{mediaId}/enable`
- **Description**: Enable/disable media item
- **Authentication**: JWT required
- **Headers**: `Authorization: Bearer {JWT}`
- **Path Parameters**: 
  - `id` - Property ID
  - `mediaId` - Media ID
- **Request Body**: `{ "enabled": true }`
- **Response**: `bool`
- **Status Codes**: 200, 400, 401, 404

### **DELETE** `/properties/{id}/media/{mediaId}`
- **Description**: Delete media item
- **Authentication**: JWT required
- **Headers**: `Authorization: Bearer {JWT}`
- **Path Parameters**: 
  - `id` - Property ID
  - `mediaId` - Media ID
- **Response**: `bool`
- **Status Codes**: 200, 400, 401, 404

## 📈 **Enhanced Statistics Endpoints**

### **GET** `/stats/properties`
- **Description**: Get comprehensive property statistics
- **Authentication**: Public endpoint (no authentication required)
- **Query Parameters**:
  - `city` (string): Filter by city
  - `propertyType` (string): Filter by property type
  - `from` (date): Start date for trends
  - `to` (date): End date for trends
- **Response**: `PropertyStatsDto`
- **Status Codes**: 200

**Response Example:**
```json
{
  "total": 150,
  "active": 120,
  "sold": 20,
  "rented": 10,
  "averagePrice": 2500000,
  "medianPrice": 2200000,
  "priceRange": {
    "min": 500000,
    "max": 10000000
  },
  "byCity": {
    "Miami Beach": 45,
    "Brickell": 30,
    "South Beach": 25
  },
  "byType": {
    "Villa": 60,
    "Apartment": 50,
    "Penthouse": 40
  },
  "byStatus": {
    "Active": 120,
    "Sold": 20,
    "Rented": 10
  },
  "trends": [
    {
      "month": "Jan 2024",
      "count": 12,
      "averagePrice": 2400000,
      "newListings": 8,
      "sold": 3,
      "rented": 1
    }
  ],
  "priceTrends": [
    {
      "period": "Last 3 months vs Previous",
      "averagePrice": 2500000,
      "priceChange": 100000,
      "priceChangePercentage": 4.2,
      "transactionCount": 25
    }
  ],
  "amenities": {
    "withPool": 80,
    "withGarden": 65,
    "withParking": 120,
    "furnished": 45,
    "poolPremium": 150000,
    "gardenPremium": 80000,
    "parkingPremium": 50000
  }
}
```

### **GET** `/stats/properties/trends`
- **Description**: Get property trends over time
- **Authentication**: JWT required
- **Headers**: `Authorization: Bearer {JWT}`
- **Query Parameters**:
  - `months` (int): Number of months (default: 12)
- **Response**: `List<MonthlyTrend>`
- **Status Codes**: 200, 401

### **GET** `/stats/properties/prices`
- **Description**: Get price trends and analysis
- **Authentication**: JWT required
- **Headers**: `Authorization: Bearer {JWT}`
- **Query Parameters**:
  - `periods` (int): Number of periods (default: 12)
- **Response**: `List<PriceTrend>`
- **Status Codes**: 200, 401

## 🔍 **Search Analytics & Suggestions**

### **GET** `/properties/search/suggestions`
- **Description**: Get search suggestions based on query
- **Authentication**: Public endpoint
- **Query Parameters**:
  - `query` (string): Search query (min 2 characters)
  - `limit` (int): Number of suggestions (default: 10)
- **Response**: `List<string>`
- **Status Codes**: 200, 400

### **GET** `/properties/search/popular`
- **Description**: Get popular search terms
- **Authentication**: Public endpoint
- **Query Parameters**:
  - `limit` (int): Number of terms (default: 10)
- **Response**: `List<string>`
- **Status Codes**: 200

### **POST** `/properties/search/analytics`
- **Description**: Get search analytics for a specific query
- **Authentication**: Public endpoint
- **Request Body**: `{ "query": "search term" }`
- **Response**: `SearchAnalytics`
- **Status Codes**: 200, 400

## 📊 **Enhanced Monitoring Endpoints**

### **GET** `/metrics`
- **Description**: Get system metrics for monitoring
- **Authentication**: Public endpoint
- **Response**: System metrics object
- **Status Codes**: 200

**Response Example:**
```json
{
  "timestamp": "2024-01-01T00:00:00Z",
  "uptime": 86400000,
  "memory": 1073741824,
  "activeConnections": 25,
  "requestsPerSecond": 150.5,
  "errorRate": 0.02
}
```

### **GET** `/health/live`
- **Description**: Liveness health check
- **Authentication**: Public endpoint
- **Response**: Health status
- **Status Codes**: 200, 503

### **GET** `/health/ready`
- **Description**: Readiness health check
- **Authentication**: Public endpoint
- **Response**: Health status
- **Status Codes**: 200, 503

## 📊 **Monitoring & Observability**

- **Health Checks**: `/health/live` and `/health/ready`
- **Metrics**: Prometheus-compatible metrics at `/metrics`
- **Logging**: Structured logging with correlation IDs
- **Tracing**: Distributed tracing support
- **Audit**: Complete audit trail for all operations
