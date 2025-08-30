# Million Properties API - Endpoints Documentation 📚

## Overview

The Million Properties API provides a complete CRUD interface for luxury real estate properties with advanced search capabilities, image management, and enterprise-grade features.

## 🔗 Base URL
```
Development: http://localhost:5000
Production: https://api.millionproperties.com
```

## 📋 API Endpoints Summary

| Method | Endpoint | Description | Status Code |
|--------|----------|-------------|-------------|
| `GET` | `/health/live` | Application health status | 200 |
| `GET` | `/health/ready` | Application readiness status | 200 |
| `GET` | `/properties` | List properties with advanced filtering | 200 |
| `GET` | `/properties/{id}` | Get property by ID | 200, 404 |
| `POST` | `/properties` | Create new property | 201, 400, 429 |
| `PUT` | `/properties/{id}` | Update property (full/partial) | 200, 400, 404, 429 |
| `DELETE` | `/properties/{id}` | Delete property | 204, 404, 429 |
| `PATCH` | `/properties/{id}/activate` | Activate property | 200, 404, 429 |
| `PATCH` | `/properties/{id}/deactivate` | Deactivate property | 200, 404, 429 |

## 🏥 Health & Status Endpoints

### **GET /health/live**
**Application health status**

**Response:**
```json
{
  "status": "ok"
}
```

**Use Cases:**
- Load balancer health checks
- Monitoring system status
- Basic application availability

---

### **GET /health/ready**
**Application readiness status**

**Response:**
```json
{
  "status": "ready"
}
```

**Use Cases:**
- Kubernetes readiness probes
- Service mesh health checks
- Database connection verification

## 🏠 Properties - CRUD Operations

### **GET /properties**
**List properties with advanced filtering and pagination**

**Query Parameters:**
| Parameter | Type | Required | Description | Example |
|-----------|------|----------|-------------|---------|
| `page` | integer | No | Page number (default: 1) | `?page=2` |
| `pageSize` | integer | No | Items per page (1-100, default: 20) | `?pageSize=50` |
| `search` | string | No | Text search across multiple fields | `?search=miami` |
| `city` | string | No | Filter by city name | `?city=miami beach` |
| `neighborhood` | string | No | Filter by neighborhood | `?neighborhood=south beach` |
| `propertyType` | string | No | Filter by property type | `?propertyType=villa` |
| `minPrice` | decimal | No | Minimum price filter | `?minPrice=1000000` |
| `maxPrice` | decimal | No | Maximum price filter | `?maxPrice=5000000` |
| `minSize` | decimal | No | Minimum size in square meters | `?minSize=200` |
| `maxSize` | decimal | No | Maximum size in square meters | `?maxSize=500` |
| `bedrooms` | integer | No | Number of bedrooms | `?bedrooms=3` |
| `bathrooms` | integer | No | Number of bathrooms | `?bathrooms=2` |
| `hasPool` | boolean | No | Has swimming pool | `?hasPool=true` |
| `hasGarden` | boolean | No | Has garden | `?hasGarden=true` |
| `hasParking` | boolean | No | Has parking | `?hasParking=true` |
| `isFurnished` | boolean | No | Is furnished | `?isFurnished=true` |
| `availableFrom` | date | No | Available from date (ISO format) | `?availableFrom=2024-01-01` |
| `availableTo` | date | No | Available to date (ISO format) | `?availableTo=2024-12-31` |
| `idOwner` | string | No | Filter by property owner ID | `?idOwner=owner123` |
| `sort` | string | No | Sort order (see sorting options) | `?sort=-price` |

**Sorting Options:**
- `price` / `-price` - Price ascending/descending
- `name` / `-name` - Name alphabetical order
- `date` / `-date` - Creation date order
- `size` / `-size` - Property size order
- `bedrooms` / `-bedrooms` - Bedroom count order
- `bathrooms` / `-bathrooms` - Bathroom count order

**Example Requests:**
```bash
# Basic pagination
GET /properties?page=1&pageSize=20

# Text search with location
GET /properties?search=miami&city=miami beach&pageSize=50

# Price and size filtering
GET /properties?minPrice=1000000&maxPrice=5000000&minSize=200&maxSize=500

# Property features
GET /properties?bedrooms=3&bathrooms=2&hasPool=true&hasGarden=true

# Advanced search with sorting
GET /properties?search=luxury&propertyType=villa&minPrice=2000000&sort=-price&page=1&pageSize=25
```

**Response:**
```json
{
  "items": [
    {
      "id": "68a6ad80c9d00d40aa74e39a",
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
  ],
  "total": 1,
  "page": 1,
  "pageSize": 20
}
```

---

### **GET /properties/{id}**
**Get property by ID**

**Path Parameters:**
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `id` | string | Yes | Property ID (MongoDB ObjectId) |

**Example Request:**
```bash
GET /properties/68a6ad80c9d00d40aa74e39a
```

**Response (200 OK):**
```json
{
  "id": "68a6ad80c9d00d40aa74e39a",
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

**Response (404 Not Found):**
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
  "title": "Resource Not Found",
  "status": 404,
  "detail": "Property 68a6ad80c9d00d40aa74e39a not found",
  "correlationId": "550e8400-e29b-41d4-a716-446655440000"
}
```

---

### **POST /properties**
**Create new property**

**Request Body:**
```json
{
  "idOwner": "owner123",
  "name": "Luxury Villa in Miami Beach",
  "description": "Stunning luxury villa with ocean views, modern amenities, and private beach access.",
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
  "image": "https://store1.public.blob.vercel-storage.com/properties/prop123/cover.jpg",
  "images": [
    "https://store1.public.blob.vercel-storage.com/properties/prop123/1.jpg",
    "https://store1.public.blob.vercel-storage.com/properties/prop123/2.jpg"
  ]
}
```

**Field Validation:**
| Field | Type | Required | Validation Rules |
|-------|------|----------|------------------|
| `idOwner` | string | Yes | Max 100 characters |
| `name` | string | Yes | Max 200 characters |
| `description` | string | No | Max 1000 characters |
| `addressProperty` | string | Yes | Max 200 characters |
| `city` | string | Yes | Max 100 characters |
| `neighborhood` | string | No | Max 100 characters |
| `propertyType` | string | Yes | Max 50 characters |
| `priceProperty` | decimal | Yes | Non-negative value |
| `size` | decimal | No | Non-negative value |
| `bedrooms` | integer | No | 0-20 range |
| `bathrooms` | integer | No | 0-20 range |
| `hasPool` | boolean | No | Default false |
| `hasGarden` | boolean | No | Default false |
| `hasParking` | boolean | No | Default false |
| `isFurnished` | boolean | No | Default false |
| `availableFrom` | datetime | No | ISO 8601 format |
| `availableTo` | datetime | No | ISO 8601 format |
| `image` | string | Yes | Valid Vercel Blob URL ending with `/cover.{ext}` |
| `images` | array | No | Max 12 items, valid Vercel Blob URLs ending with `/{index}.{ext}` |

**Response (201 Created):**
```json
{
  "id": "68a6ad80c9d00d40aa74e39a",
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

**Response (400 Bad Request):**
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Validation Failed",
  "status": 400,
  "detail": "Cover image must be a valid Vercel Blob URL; Gallery cannot exceed 12 images",
  "correlationId": "550e8400-e29b-41d4-a716-446655440000",
  "errors": {
    "Image": ["Cover image must be a valid Vercel Blob URL"],
    "Images": ["Gallery cannot exceed 12 images"]
  }
}
```

---

### **PUT /properties/{id}**
**Update property (full or partial update)**

**Path Parameters:**
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `id` | string | Yes | Property ID (MongoDB ObjectId) |

**Request Body:**
```json
{
  "name": "Updated Luxury Villa in Miami Beach",
  "description": "Recently renovated luxury villa with enhanced ocean views and new modern amenities.",
  "priceProperty": 2750000.00,
  "hasPool": true,
  "hasGarden": true,
  "isFurnished": true
}
```

**Field Validation:**
| Field | Type | Required | Validation Rules |
|-------|------|----------|------------------|
| `name` | string | No | Max 200 characters |
| `description` | string | No | Max 1000 characters |
| `addressProperty` | string | No | Max 200 characters |
| `city` | string | No | Max 100 characters |
| `neighborhood` | string | No | Max 100 characters |
| `propertyType` | string | No | Max 50 characters |
| `priceProperty` | decimal | No | Non-negative value |
| `size` | decimal | No | Non-negative value |
| `bedrooms` | integer | No | 0-20 range |
| `bathrooms` | integer | No | 0-20 range |
| `hasPool` | boolean | No | Boolean value |
| `hasGarden` | boolean | No | Boolean value |
| `hasParking` | boolean | No | Boolean value |
| `isFurnished` | boolean | No | Boolean value |
| `availableFrom` | datetime | No | ISO 8601 format |
| `availableTo` | datetime | No | ISO 8601 format |
| `image` | string | No | Valid Vercel Blob URL ending with `/cover.{ext}` |
| `images` | array | No | Max 12 items, valid Vercel Blob URLs ending with `/{index}.{ext}` |
| `isActive` | boolean | No | Boolean value |

**Response (200 OK):**
```json
{
  "id": "68a6ad80c9d00d40aa74e39a",
  "idOwner": "owner123",
  "name": "Updated Luxury Villa in Miami Beach",
  "description": "Recently renovated luxury villa with enhanced ocean views...",
  "addressProperty": "123 Ocean Drive, Miami Beach, FL 33139",
  "city": "Miami Beach",
  "neighborhood": "South Beach",
  "propertyType": "Villa",
  "priceProperty": 2750000.00,
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
  "updatedAt": "2024-01-15T11:45:00Z",
  "image": "https://store1.public.blob.vercel-storage.com/properties/prop123/cover.jpg",
  "images": [
    "https://store1.public.blob.vercel-storage.com/properties/prop123/1.jpg",
    "https://store1.public.blob.vercel-storage.com/properties/prop123/2.jpg"
  ],
  "isActive": true
}
```

**Response (404 Not Found):**
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
  "title": "Resource Not Found",
  "status": 404,
  "detail": "Property 68a6ad80c9d00d40aa74e39a not found",
  "correlationId": "550e8400-e29b-41d4-a716-446655440000"
}
```

---

### **DELETE /properties/{id}**
**Delete property**

**Path Parameters:**
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `id` | string | Yes | Property ID (MongoDB ObjectId) |

**Example Request:**
```bash
DELETE /properties/68a6ad80c9d00d40aa74e39a
```

**Response (204 No Content):**
*No response body*

**Response (404 Not Found):**
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
  "title": "Resource Not Found",
  "status": 404,
  "detail": "Property 68a6ad80c9d00d40aa74e39a not found",
  "correlationId": "550e8400-e29b-41d4-a716-446655440000"
}
```

---

### **PATCH /properties/{id}/activate**
**Activate property**

**Path Parameters:**
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `id` | string | Yes | Property ID (MongoDB ObjectId) |

**Example Request:**
```bash
PATCH /properties/68a6ad80c9d00d40aa74e39a/activate
```

**Response (200 OK):**
```json
{
  "message": "Property activated successfully"
}
```

---

### **PATCH /properties/{id}/deactivate**
**Deactivate property**

**Path Parameters:**
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `id` | string | Yes | Property ID (MongoDB ObjectId) |

**Example Request:**
```bash
PATCH /properties/68a6ad80c9d00d40aa74e39a/deactivate
```

**Response (200 OK):**
```json
{
  "message": "Property deactivated successfully"
}
```

## 🖼️ Vercel Blob - Image Management

### **POST /api/blob/upload**
**Get upload URL for property images**

**Base URL:** `http://localhost:3000` (Next.js service)

**Request Body:**
```json
{
  "propertyId": "68a6ad80c9d00d40aa74e39a",
  "kind": "cover",
  "index": null
}
```

**Field Validation:**
| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `propertyId` | string | Yes | Property ID for image association |
| `kind` | string | Yes | Image type: "cover" or "gallery" |
| `index` | integer | No | Gallery index (1-12) when kind is "gallery" |

**Response (200 OK):**
```json
{
  "url": "https://vercel-blob-upload-url.com/...",
  "pathname": "properties/68a6ad80c9d00d40aa74e39a/cover.jpg",
  "contentType": "image/jpeg",
  "propertyId": "68a6ad80c9d00d40aa74e39a",
  "kind": "cover"
}
```

**Response (400 Bad Request):**
```json
{
  "error": "gallery index must be between 1 and 12"
}
```

---

### **GET /api/blob/upload**
**Get upload endpoint information**

**Response (200 OK):**
```json
{
  "message": "Vercel Blob upload endpoint",
  "maxGalleryImages": 12,
  "usage": {
    "cover": "POST with { propertyId, kind: \"cover\" }",
    "gallery": "POST with { propertyId, kind: \"gallery\", index: 1-12 }"
  }
}
```

## 🔒 Error Handling

### **HTTP Status Codes**
| Status Code | Description | Use Case |
|-------------|-------------|----------|
| `200 OK` | Request successful | GET, PUT, PATCH operations |
| `201 Created` | Resource created successfully | POST operations |
| `204 No Content` | Request successful, no response body | DELETE operations |
| `400 Bad Request` | Invalid request data | Validation errors |
| `404 Not Found` | Resource not found | Invalid ID or missing resource |
| `429 Too Many Requests` | Rate limit exceeded | Too many requests per minute |
| `500 Internal Server Error` | Server error | Unexpected server errors |

### **Error Response Format (RFC 7807)**
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Validation Failed",
  "status": 400,
  "detail": "Detailed error message",
  "correlationId": "550e8400-e29b-41d4-a716-446655440000",
  "errors": {
    "fieldName": ["Field-specific error message"]
  }
}
```

### **Validation Error Examples**

#### **Missing Required Fields**
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Validation Failed",
  "status": 400,
  "detail": "IdOwner is required; Name is required; AddressProperty is required; PropertyType is required; PriceProperty is required; Cover image is required",
  "correlationId": "550e8400-e29b-41d4-a716-446655440000"
}
```

#### **Invalid Image URLs**
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Validation Failed",
  "status": 400,
  "detail": "Cover image must be a valid Vercel Blob URL; Cover image path must follow pattern '/prop-{id}_photo-01.jpg'",
  "correlationId": "550e8400-e29b-41d4-a716-446655440000"
}
```

#### **Invalid Query Parameters**
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Validation Failed",
  "status": 400,
  "detail": "Page must be greater than 0; PageSize must be between 1 and 100",
  "correlationId": "550e8400-e29b-41d4-a716-446655440000"
}
```

## 🌐 CORS Configuration

### **Allowed Origins**
- Development: `http://localhost:3000`
- Production: Configurable via `CORS_ORIGINS` environment variable

### **CORS Headers**
- `Access-Control-Allow-Origin`: Configured origins
- `Access-Control-Allow-Methods`: GET, POST, PUT, DELETE, PATCH
- `Access-Control-Allow-Headers`: Content-Type, X-Correlation-ID
- `Access-Control-Allow-Credentials`: false

## 🚦 Rate Limiting

### **Configuration**
- **Default Limit**: 120 requests per minute per IP
- **Configurable**: Via `RATE_LIMIT_PERMINUTE` environment variable
- **Headers**: `Retry-After: 60` when limit exceeded

### **Rate Limit Response**
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.8",
  "title": "Too Many Requests",
  "status": 429,
  "detail": "Rate limit exceeded",
  "correlationId": "550e8400-e29b-41d4-a716-446655440000"
}
```

## 🔗 Correlation IDs

### **Request Tracing**
- **Header**: `X-Correlation-ID`
- **Auto-generated**: If not provided by client
- **Included**: In all error responses and logs
- **Format**: UUID v4

### **Example Usage**
```bash
# Set correlation ID
curl -H "X-Correlation-ID: my-request-123" \
     -H "Content-Type: application/json" \
     -X POST \
     -d '{"name":"Test Property"}' \
     http://localhost:5000/properties

# Response includes correlation ID
{
  "id": "68a6ad80c9d00d40aa74e39a",
  "name": "Test Property",
  # ... other fields
}
```

## 📊 Response Headers

### **Standard Headers**
- `Content-Type`: `application/json`
- `X-Correlation-ID`: Request correlation ID
- `Retry-After`: Rate limit information (when applicable)

### **Pagination Headers**
- `X-Total-Count`: Total number of items
- `X-Page`: Current page number
- `X-Page-Size`: Items per page

## 👤 **Owners Endpoints**

### **GET /owners/profile**
**Get authenticated owner profile**

**Authentication:** Required (JWT Token)

**Request Headers:**
```http
Authorization: Bearer {JWT_TOKEN}
Content-Type: application/json
```

**Response (200 OK):**
```json
{
  "id": "owner-001",
  "fullName": "Carlos Rodriguez",
  "email": "carlos.rodriguez@million.com",
  "phoneE164": "+13055551234",
  "photoUrl": "https://blob.vercel-storage.com/owners/carlos-rodriguez.jpg",
  "role": "Owner",
  "isActive": true,
  "createdAt": "2024-01-01T00:00:00Z",
  "updatedAt": "2024-01-01T00:00:00Z"
}
```

**Response (401 Unauthorized):**
```json
{
  "type": "https://tools.ietf.org/html/rfc7235#section-3.1",
  "title": "Unauthorized",
  "status": 401,
  "detail": "Missing or invalid authorization token",
  "instance": "/owners/profile",
  "timestamp": "2024-01-01T00:00:00Z",
  "extensions": {
    "correlationId": "uuid",
    "requestId": "request-id",
    "endpoint": "/owners/profile",
    "method": "GET",
    "clientIp": "127.0.0.1"
  }
}
```

---

## 🔍 Search Examples
```bash
# Search for properties with "miami" in name, description, or address
GET /properties?search=miami&page=1&pageSize=20
```

### **Advanced Filtering**
```bash
# Luxury villas in Miami Beach with pool, 3+ bedrooms, under $3M
GET /properties?search=luxury&city=miami beach&propertyType=villa&hasPool=true&bedrooms=3&maxPrice=3000000&sort=-price
```

### **Location-Based Search**
```bash
# Properties in Brickell neighborhood with parking
GET /properties?location=brickell&hasParking=true&sort=price
```

### **Amenity Search**
```bash
# Furnished properties with garden and parking
GET /properties?isFurnished=true&hasGarden=true&hasParking=true&sort=-date
```

### **Price Range Search**
```bash
# Properties between $1M and $5M, sorted by price
GET /properties?minPrice=1000000&maxPrice=5000000&sort=price
```

### **Size and Feature Search**
```bash
# Large properties (300+ sqm) with 4+ bedrooms
GET /properties?minSize=300&bedrooms=4&sort=-size
```

## 📝 Usage Guidelines

### **Best Practices**
1. **Always include correlation IDs** for request tracing
2. **Use pagination** for large result sets
3. **Validate responses** and handle errors gracefully
4. **Respect rate limits** and implement exponential backoff
5. **Cache responses** when appropriate

### **Performance Tips**
1. **Use specific filters** to reduce result sets
2. **Limit page sizes** to reasonable values (20-50)
3. **Combine filters** for more targeted searches
4. **Use sorting** to get most relevant results first

### **Error Handling**
1. **Check status codes** before processing responses
2. **Parse error details** for user-friendly messages
3. **Log correlation IDs** for debugging
4. **Implement retry logic** for transient errors

---

**This documentation provides comprehensive information for integrating with the Million Properties API.**
