# 🚀 **API Integration Guide - Million Real Estate API**

## 📋 **Table of Contents**
1. [Quick Start](#quick-start)
2. [Authentication](#authentication)
3. [Data Models](#data-models)
4. [Endpoints Reference](#endpoints-reference)
5. [Error Handling](#error-handling)
6. [Rate Limiting](#rate-limiting)
7. [Best Practices](#best-practices)
8. [Code Examples](#code-examples)
9. [Testing](#testing)
10. [Support](#support)

## 🚀 **Quick Start**

### **Base URL**
```bash
https://million-real-estate-api-sh25jnp3aa-uc.a.run.app
```

### **Health Check**
```bash
curl "https://million-real-estate-api-sh25jnp3aa-uc.a.run.app/health/live"
```

### **Get Properties (No Auth Required)**
```bash
curl "https://million-real-estate-api-sh25jnp3aa-uc.a.run.app/properties?page=1&pageSize=5"
```

## 🔐 **Authentication**

### **JWT Token Flow**
1. **Login** → Get JWT + Refresh Token
2. **Use JWT** → Include in `Authorization` header
3. **Refresh** → Get new JWT when expired
4. **Logout** → Invalidate refresh token

### **Login Request**
```bash
curl -X POST "https://million-real-estate-api-sh25jnp3aa-uc.a.run.app/auth/login" \
  -H "Content-Type: application/json" \
  -d '{
    "email": "carlos.rodriguez@million.com",
    "password": "your-password"
  }'
```

### **Response Format**
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "refresh-token-here",
  "expiresIn": 3600,
  "tokenType": "Bearer"
}
```

### **Using JWT Token**
```bash
curl -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  "https://million-real-estate-api-sh25jnp3aa-uc.a.run.app/owners/profile"
```

## 📊 **Data Models**

### **Property Object**
```json
{
  "id": "prop-001",
  "ownerId": "owner-001",
  "name": "Oceanfront Villa",
  "address": "100 Ocean Dr, Miami Beach, FL",
  "city": "Miami Beach",
  "neighborhood": "South Beach",
  "propertyType": "Villa",
  "description": "Luxurious oceanfront villa with stunning panoramic views of the Atlantic Ocean. This 5-bedroom, 6-bathroom estate features premium finishes, a private infinity pool, direct beach access, and state-of-the-art smart home technology. Perfect for luxury living and entertaining.",
  "price": 12500000,
  "codeInternal": "MB001",
  "year": 2020,
  "size": 8500,
  "bedrooms": 5,
  "bathrooms": 6,
  "hasPool": true,
  "hasGarden": true,
  "hasParking": true,
  "isFurnished": true,
  "availableFrom": "2024-01-15T00:00:00Z",
  "availableTo": "2024-12-31T00:00:00Z",
  "status": "Active",
  "cover": {
    "type": "Image",
    "url": "https://0daikfjw6ec1yprw.public.blob.vercel-storage.com/properties/mb001/cover.jpg",
    "index": 0
  },
  "media": [
    {
      "id": "media-001",
      "type": "Image",
      "url": "https://0daikfjw6ec1yprw.public.blob.vercel-storage.com/properties/mb001/1.jpg",
      "index": 1,
      "enabled": true,
      "featured": true
    }
  ],
  "createdAt": "2024-01-01T00:00:00Z",
  "updatedAt": "2024-01-01T00:00:00Z",
  "isActive": true
}
```

**Field Validation Rules:**
| Field | Type | Required | Validation Rules |
|-------|------|----------|------------------|
| `description` | string | No | Max 1000 characters |
| `name` | string | Yes | Max 200 characters |
| `address` | string | Yes | Max 200 characters |
| `price` | decimal | Yes | Non-negative value |
| `year` | integer | No | 1800-2100 range |
| `size` | decimal | No | Non-negative value |
| `bedrooms` | integer | No | 0-20 range |
| `bathrooms` | integer | No | 0-20 range |

### **Owner Object**
```json
{
  "id": "owner-001",
  "fullName": "Carlos Rodriguez",
  "email": "carlos.rodriguez@million.com",
  "phoneE164": "+13055551234",
  "photoUrl": "https://0daikfjw6ec1yprw.public.blob.vercel-storage.com/owners/carlos-rodriguez.jpg",
  "role": "Owner",
  "isActive": true,
  "createdAt": "2024-01-01T00:00:00Z",
  "updatedAt": "2024-01-01T00:00:00Z"
}
```

### **Media Object**
```json
{
  "id": "media-001",
  "type": "Image",
  "url": "https://0daikfjw6ec1yprw.public.blob.vercel-storage.com/properties/mb001/1.jpg",
  "index": 1,
  "enabled": true,
  "featured": true
}
```

## 🛣️ **Endpoints Reference**

### **Properties**

#### **List Properties**
```bash
GET /properties?page=1&pageSize=10&city=Miami&minPrice=1000000
```

**Query Parameters:**
- `page`: Page number (default: 1)
- `pageSize`: Items per page (default: 10, max: 100)
- `search`: Search in name, description, address (full-text search)
- `city`: Filter by city
- `neighborhood`: Filter by neighborhood
- `propertyType`: Filter by property type
- `minPrice`/`maxPrice`: Price range
- `bedrooms`/`bathrooms`: Room count
- `minSize`/`maxSize`: Size range
- `hasPool`/`hasGarden`/`hasParking`/`isFurnished`: Boolean filters
- `availableFrom`/`availableTo`: Date range
- `sort`: Sort field (name, price, year, createdAt, description)

#### **Get Property by ID**
```bash
GET /properties/{id}
```

#### **Create Property**
```bash
POST /properties
Authorization: Bearer {JWT}
Content-Type: application/json

{
  "name": "New Property",
  "description": "Beautiful property with modern amenities and excellent location. Features premium finishes and quality construction.",
  "address": "123 Main St",
  "city": "Miami",
  "price": 500000,
  "propertyType": "Apartment",
  "bedrooms": 2,
  "bathrooms": 2
}
```

**Required Fields:**
- `name`: Property name (max 200 characters)
- `address`: Property address (max 200 characters)
- `city`: City (max 100 characters)
- `price`: Price (non-negative decimal)
- `propertyType`: Type of property (max 50 characters)

**Optional Fields:**
- `description`: Detailed description (max 1000 characters)
- `neighborhood`: Neighborhood (max 100 characters)
- `size`: Property size in sqm (non-negative decimal)
- `bedrooms`: Number of bedrooms (0-20 range)
- `bathrooms`: Number of bathrooms (0-20 range)
- `hasPool`, `hasGarden`, `hasParking`, `isFurnished`: Boolean amenities
- `availableFrom`, `availableTo`: Availability dates (ISO 8601 format)

#### **Update Property**
```bash
PUT /properties/{id}
Authorization: Bearer {JWT}
Content-Type: application/json

{
  "name": "Updated Property Name",
  "description": "Recently renovated property with enhanced features and modern amenities. Includes updated kitchen and bathrooms.",
  "price": 550000,
  "hasPool": true,
  "isFurnished": true
}
```

**Updateable Fields:**
- `name`: Property name (max 200 characters)
- `description`: Detailed description (max 1000 characters)
- `address`: Property address (max 200 characters)
- `city`: City (max 100 characters)
- `neighborhood`: Neighborhood (max 100 characters)
- `propertyType`: Type of property (max 50 characters)
- `price`: Price (non-negative decimal)
- `size`: Property size in sqm (non-negative decimal)
- `bedrooms`: Number of bedrooms (0-20 range)
- `bathrooms`: Number of bathrooms (0-20 range)
- `hasPool`, `hasGarden`, `hasParking`, `isFurnished`: Boolean amenities
- `availableFrom`, `availableTo`: Availability dates (ISO 8601 format)
- `isActive`: Property availability status

#### **Delete Property**
```bash
DELETE /properties/{id}
Authorization: Bearer {JWT}
```

### **Authentication**

#### **Login**
```bash
POST /auth/login
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "password123"
}
```

#### **Refresh Token**
```bash
POST /auth/refresh
Content-Type: application/json

{
  "refreshToken": "your-refresh-token"
}
```

#### **Logout**
```bash
POST /auth/logout
Authorization: Bearer {JWT}
```

### **Owners**

#### **Get Profile**
```bash
GET /owners/profile
Authorization: Bearer {JWT}
```

**Response (200 OK):**
```json
{
  "id": "owner-001",
  "fullName": "Carlos Rodriguez",
  "email": "carlos.rodriguez@million.com",
  "phoneE164": "+13055551234",
  "photoUrl": "https://0daikfjw6ec1yprw.public.blob.vercel-storage.com/owners/carlos-rodriguez.jpg",
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

#### **Update Profile**
```bash
PUT /owners/profile
Authorization: Bearer {JWT}
Content-Type: application/json

{
  "fullName": "New Name",
  "phoneE164": "+13055559999",
  "photoUrl": "https://0daikfjw6ec1yprw.public.blob.vercel-storage.com/owners/new-photo.jpg"
}
```

**Updateable Fields:**
- `fullName`: Owner's full name (max 200 characters)
- `phoneE164`: Phone number in E.164 format (max 20 characters)
- `photoUrl`: Profile photo URL (max 500 characters)

## 📝 **Field Validation & Constraints**

### **Description Field**
The `description` field is a powerful feature that allows detailed property descriptions:

**Features:**
- **Maximum Length**: 1000 characters
- **Searchable**: Included in full-text search across name, description, and address
- **Optional**: Not required for property creation
- **Rich Content**: Supports detailed property features, amenities, and selling points

**Best Practices:**
- Use descriptive language to highlight unique features
- Include key amenities and selling points
- Mention location benefits and neighborhood highlights
- Keep descriptions engaging and informative
- Use proper grammar and punctuation

**Example Descriptions:**
```json
{
  "description": "Luxurious oceanfront villa with stunning panoramic views of the Atlantic Ocean. This 5-bedroom, 6-bathroom estate features premium finishes, a private infinity pool, direct beach access, and state-of-the-art smart home technology. Perfect for luxury living and entertaining."
}
```

```json
{
  "description": "Modern apartment in the heart of downtown with city skyline views. Features open floor plan, high-end appliances, fitness center access, and convenient parking. Walking distance to restaurants, shopping, and public transportation."
}
```

---

## 🚨 **Error Handling**

### **HTTP Status Codes**
- `200` - Success
- `201` - Created
- `204` - No Content
- `400` - Bad Request
- `401` - Unauthorized
- `403` - Forbidden
- `404` - Not Found
- `429` - Too Many Requests
- `500` - Internal Server Error

### **Error Response Format**
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

### **Common Error Scenarios**

#### **Validation Errors**
```json
{
  "title": "Validation Failed",
  "detail": "The Name field is required.",
  "errors": {
    "Name": ["The Name field is required."],
    "Price": ["Price must be greater than 0."]
  }
}
```

#### **Authentication Errors**
```json
{
  "title": "Unauthorized",
  "detail": "Invalid or expired token"
}
```

#### **Rate Limiting**
```json
{
  "title": "Too Many Requests",
  "detail": "Rate limit exceeded. Try again in 60 seconds."
}
```

## 📊 **Rate Limiting**

### **Limits**
- **100 requests per minute** per IP address
- **1000 requests per hour** per IP address

### **Headers**
```http
X-RateLimit-Limit: 100
X-RateLimit-Remaining: 95
X-RateLimit-Reset: 1640995200
```

### **Handling Rate Limits**
```javascript
if (response.status === 429) {
  const resetTime = response.headers.get('X-RateLimit-Reset');
  const waitTime = resetTime - Date.now() / 1000;
  
  if (waitTime > 0) {
    await new Promise(resolve => setTimeout(resolve, waitTime * 1000));
  }
  
  // Retry request
}
```

## 💡 **Best Practices**

### **Authentication**
1. **Store tokens securely** - Use secure storage (not localStorage)
2. **Refresh proactively** - Refresh JWT before expiration
3. **Handle 401 errors** - Redirect to login when token expires
4. **Secure refresh tokens** - Store in httpOnly cookies if possible

### **Error Handling**
1. **Always check status codes** - Don't assume success
2. **Parse error responses** - Extract meaningful error messages
3. **Implement retry logic** - For transient failures
4. **Log errors** - For debugging and monitoring

### **Performance**
1. **Use pagination** - Don't fetch all data at once
2. **Implement caching** - Cache frequently accessed data
3. **Optimize requests** - Use filters to reduce data transfer
4. **Handle loading states** - Show loading indicators

### **Security**
1. **Validate input** - Always validate user input
2. **Sanitize data** - Clean data before sending to API
3. **Use HTTPS** - Always use secure connections
4. **Implement timeouts** - Set reasonable request timeouts

## 💻 **Code Examples**

### **JavaScript/TypeScript**

#### **API Client Class**
```typescript
class MillionAPI {
  private baseUrl = 'https://million-real-estate-api-sh25jnp3aa-uc.a.run.app';
  private accessToken: string | null = null;
  private refreshToken: string | null = null;

  async login(email: string, password: string) {
    const response = await fetch(`${this.baseUrl}/auth/login`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ email, password })
    });

    if (!response.ok) {
      throw new Error('Login failed');
    }

    const data = await response.json();
    this.accessToken = data.accessToken;
    this.refreshToken = data.refreshToken;
    
    return data;
  }

  async getProperties(filters: PropertyFilters = {}) {
    const params = new URLSearchParams();
    
    Object.entries(filters).forEach(([key, value]) => {
      if (value !== undefined && value !== null) {
        params.append(key, value.toString());
      }
    });

    const response = await fetch(
      `${this.baseUrl}/properties?${params}`,
      {
        headers: this.getAuthHeaders()
      }
    );

    if (!response.ok) {
      throw new Error('Failed to fetch properties');
    }

    return response.json();
  }

  private getAuthHeaders() {
    const headers: Record<string, string> = {
      'Content-Type': 'application/json'
    };

    if (this.accessToken) {
      headers['Authorization'] = `Bearer ${this.accessToken}`;
    }

    return headers;
  }

  async refreshAccessToken() {
    if (!this.refreshToken) {
      throw new Error('No refresh token available');
    }

    const response = await fetch(`${this.baseUrl}/auth/refresh`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ refreshToken: this.refreshToken })
    });

    if (!response.ok) {
      throw new Error('Token refresh failed');
    }

    const data = await response.json();
    this.accessToken = data.accessToken;
    this.refreshToken = data.refreshToken;
    
    return data;
  }
}
```

#### **Usage Example**
```typescript
const api = new MillionAPI();

// Login
await api.login('user@example.com', 'password123');

// Get properties
const properties = await api.getProperties({
  city: 'Miami',
  minPrice: 1000000,
  page: 1,
  pageSize: 10
});

console.log(`Found ${properties.total} properties`);
properties.items.forEach(property => {
  console.log(`${property.name} - $${property.price}`);
});
```

### **Python**

#### **API Client Class**
```python
import requests
import json
from typing import Dict, Any, Optional

class MillionAPI:
    def __init__(self):
        self.base_url = "https://million-real-estate-api-sh25jnp3aa-uc.a.run.app"
        self.access_token = None
        self.refresh_token = None
        self.session = requests.Session()

    def login(self, email: str, password: str) -> Dict[str, Any]:
        response = self.session.post(
            f"{self.base_url}/auth/login",
            json={"email": email, "password": password}
        )
        response.raise_for_status()
        
        data = response.json()
        self.access_token = data["accessToken"]
        self.refresh_token = data["refreshToken"]
        
        return data

    def get_properties(self, filters: Optional[Dict[str, Any]] = None) -> Dict[str, Any]:
        if filters is None:
            filters = {}
        
        headers = self._get_auth_headers()
        response = self.session.get(
            f"{self.base_url}/properties",
            params=filters,
            headers=headers
        )
        response.raise_for_status()
        
        return response.json()

    def create_property(self, property_data: Dict[str, Any]) -> Dict[str, Any]:
        headers = self._get_auth_headers()
        response = self.session.post(
            f"{self.base_url}/properties",
            json=property_data,
            headers=headers
        )
        response.raise_for_status()
        
        return response.json()

    def _get_auth_headers(self) -> Dict[str, str]:
        headers = {"Content-Type": "application/json"}
        if self.access_token:
            headers["Authorization"] = f"Bearer {self.access_token}"
        return headers

    def refresh_access_token(self) -> Dict[str, Any]:
        if not self.refresh_token:
            raise ValueError("No refresh token available")
        
        response = self.session.post(
            f"{this.base_url}/auth/refresh",
            json={"refreshToken": self.refresh_token}
        )
        response.raise_for_status()
        
        data = response.json()
        self.access_token = data["accessToken"]
        self.refresh_token = data["refreshToken"]
        
        return data
```

#### **Usage Example**
```python
api = MillionAPI()

# Login
api.login("user@example.com", "password123")

# Get properties
properties = api.get_properties({
    "city": "Miami",
    "minPrice": 1000000,
    "page": 1,
    "pageSize": 10
})

print(f"Found {properties['total']} properties")
for property in properties['items']:
    print(f"{property['name']} - ${property['price']}")

# Create property
new_property = api.create_property({
    "name": "New Property",
    "address": "123 Main St",
    "city": "Miami",
    "price": 500000,
    "propertyType": "Apartment",
    "bedrooms": 2,
    "bathrooms": 2
})
```

### **cURL Examples**

#### **Get Properties**
```bash
curl "https://million-real-estate-api-sh25jnp3aa-uc.a.run.app/properties?page=1&pageSize=5&city=Miami"
```

#### **Create Property**
```bash
curl -X POST "https://million-real-estate-api-sh25jnp3aa-uc.a.run.app/properties" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "New Property",
    "address": "123 Main St",
    "city": "Miami",
    "price": 500000,
    "propertyType": "Apartment",
    "bedrooms": 2,
    "bathrooms": 2
  }'
```

#### **Update Property**
```bash
curl -X PUT "https://million-real-estate-api-sh25jnp3aa-uc.a.run.app/properties/prop-001" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "price": 550000
  }'
```

## 🧪 **Testing**

### **Test Credentials**
```json
{
  "email": "carlos.rodriguez@million.com",
  "password": "test-password"
}
```

### **Test Properties**
- **ID**: `prop-001` - Oceanfront Villa
- **ID**: `prop-002` - Downtown Penthouse  
- **ID**: `prop-003` - Garden Estate

### **Postman Collection**
Import the Postman collection from `docs/Million-Properties-API.postman_collection.json`

### **Testing Checklist**
- [ ] Health checks work
- [ ] Authentication flow works
- [ ] CRUD operations work
- [ ] Error handling works
- [ ] Rate limiting works
- [ ] Validation works
- [ ] Pagination works
- [ ] Filtering works

## 📞 **Support**

### **Documentation**
- **API Routes**: `docs/API_ROUTES.md`
- **Architecture**: `docs/ARCHITECTURE.md`
- **Error Handling**: `docs/ERROR_HANDLING.md`

### **Contact**
- **Email**: support@million.com
- **GitHub Issues**: [Repository Issues](https://github.com/your-repo/issues)

### **Status Page**
- **Health**: `/health/live`
- **Readiness**: `/health/ready`

### **Monitoring**
- **Logs**: Google Cloud Logging
- **Metrics**: Prometheus-compatible endpoints
- **Tracing**: Distributed tracing support

---

## 📝 **Changelog**

### **v1.0.0** - 2024-01-01
- Initial API release
- Property CRUD operations
- Authentication system
- Media management
- Search and filtering

---

**Last Updated**: January 2024  
**API Version**: v1.0.0  
**Base URL**: `https://million-real-estate-api-sh25jnp3aa-uc.a.run.app`
