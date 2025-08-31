# 🏠 **Million Real Estate API - Flujo de Dashboard de Usuario**

## 📋 **Índice**

- [1. Autenticación y Sesión](#1-autenticación-y-sesión)
- [2. Información del Usuario](#2-información-del-usuario)
- [3. Gestión de Propiedades del Usuario](#3-gestión-de-propiedades-del-usuario)
- [4. CRUD de Imágenes (Media Management)](#4-crud-de-imágenes-media-management)
- [5. Búsqueda y Filtros](#5-búsqueda-y-filtros)
- [6. Estadísticas del Usuario](#6-estadísticas-del-usuario)
- [7. Códigos de Error Comunes](#7-códigos-de-error-comunes)
- [8. Headers Requeridos](#8-headers-requeridos)
- [9. Ejemplo de Flujo Completo](#9-ejemplo-de-flujo-completo)
- [10. Mejores Prácticas](#10-mejores-prácticas)

---

## 🔐 **1. Autenticación y Sesión**

### **Login de Usuario**
```http
POST /auth/owner/login
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "password123"
}
```

**Respuesta Exitosa:**
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "refresh_token_here",
  "expiresIn": 3600,
  "owner": {
    "id": "owner-001",
    "fullName": "John Doe",
    "email": "user@example.com",
    "phone": "+1234567890",
    "photoUrl": "https://vercel-blob.com/owners/owner-001/photo.jpg"
  }
}
```

### **Refresh Token**
```http
POST /auth/owner/refresh
Content-Type: application/json

{
  "refreshToken": "refresh_token_here"
}
```

---

## 👤 **2. Información del Usuario**

### **Obtener Perfil del Usuario Autenticado**
```http
GET /auth/owner/profile
Authorization: Bearer {accessToken}
```

**Respuesta:**
```json
{
  "id": "owner-001",
  "fullName": "John Doe",
  "email": "user@example.com",
  "phone": "+1234567890",
  "photoUrl": "https://vercel-blob.com/owners/owner-001/photo.jpg",
  "bio": "Experienced real estate investor",
  "company": "Doe Properties LLC",
  "website": "https://doeproperties.com",
  "socialMedia": {
    "linkedin": "https://linkedin.com/in/johndoe",
    "twitter": "https://twitter.com/johndoe"
  },
  "createdAt": "2024-01-01T00:00:00Z",
  "updatedAt": "2024-01-15T10:30:00Z"
}
```

### **Actualizar Perfil del Usuario**
```http
PUT /auth/owner/profile
Authorization: Bearer {accessToken}
Content-Type: application/json

{
  "fullName": "John Doe Updated",
  "phone": "+1234567890",
  "bio": "Updated bio text",
  "company": "Doe Properties LLC",
  "website": "https://doeproperties.com",
  "socialMedia": {
    "linkedin": "https://linkedin.com/in/johndoe",
    "twitter": "https://twitter.com/johndoe"
  }
}
```

---

## 🏠 **3. Gestión de Propiedades del Usuario**

### **Listar Propiedades del Usuario**
```http
GET /auth/owner/properties?page=1&pageSize=20
Authorization: Bearer {accessToken}
```

**Respuesta:**
```json
{
  "items": [
    {
      "id": "prop-001",
      "name": "Villa Ocean Club",
      "address": "1786 Ocean Club Dr, Key Biscayne, FL",
      "price": 2622745,
      "status": "Active",
      "coverImage": "https://vercel-blob.com/properties/prop-001/cover.jpg",
      "createdAt": "2024-01-01T00:00:00Z"
    }
  ],
  "totalCount": 1,
  "page": 1,
  "pageSize": 20,
  "totalPages": 1
}
```

### **Obtener Propiedad Detallada**
```http
GET /auth/owner/properties/{id}
Authorization: Bearer {accessToken}
```

---

## 🖼️ **4. CRUD de Imágenes (Media Management)**

### **4.1. Actualizar Imagen de Portada (Cover)**

#### **Cambiar Imagen de Portada**
```http
PATCH /properties/{id}/cover
Authorization: Bearer {accessToken}
Content-Type: application/json

{
  "url": "https://vercel-blob.com/properties/prop-001/new-cover.jpg",
  "type": "Image"
}
```

**Respuesta:**
```json
{
  "id": "prop-001",
  "cover": {
    "type": "Image",
    "url": "https://vercel-blob.com/properties/prop-001/new-cover.jpg",
    "index": 0
  },
  "coverImage": "https://vercel-blob.com/properties/prop-001/new-cover.jpg",
  "coverUrl": "https://vercel-blob.com/properties/prop-001/new-cover.jpg"
}
```

#### **Cambiar a Video de Portada**
```http
PATCH /properties/{id}/cover
Authorization: Bearer {accessToken}
Content-Type: application/json

{
  "url": "https://vercel-blob.com/properties/prop-001/cover-video.mp4",
  "type": "Video",
  "poster": "https://vercel-blob.com/properties/prop-001/cover-poster.jpg"
}
```

### **4.2. Gestión de Galería de Imágenes**

#### **Actualizar Galería Completa**
```http
PATCH /properties/{id}/gallery
Authorization: Bearer {accessToken}
Content-Type: application/json

{
  "gallery": [
    {
      "url": "https://vercel-blob.com/properties/prop-001/photo-01.jpg",
      "type": "Image",
      "index": 1,
      "featured": true
    },
    {
      "url": "https://vercel-blob.com/properties/prop-001/photo-02.jpg",
      "type": "Image",
      "index": 2,
      "featured": false
    },
    {
      "url": "https://vercel-blob.com/properties/prop-001/video-01.mp4",
      "type": "Video",
      "index": 3,
      "featured": true,
      "poster": "https://vercel-blob.com/properties/prop-001/video-01-poster.jpg"
    }
  ]
}
```

#### **Agregar Imagen Individual**
```http
PATCH /properties/{id}/media
Authorization: Bearer {accessToken}
Content-Type: application/json

{
  "action": "Add",
  "media": {
    "url": "https://vercel-blob.com/properties/prop-001/new-photo.jpg",
    "type": "Image",
    "index": 4,
    "featured": false
  }
}
```

#### **Actualizar Imagen Existente**
```http
PATCH /properties/{id}/media
Authorization: Bearer {accessToken}
Content-Type: application/json

{
  "action": "Update",
  "mediaId": "media-001",
  "updates": {
    "featured": true,
    "index": 1
  }
}
```

#### **Eliminar Imagen**
```http
PATCH /properties/{id}/media
Authorization: Bearer {accessToken}
Content-Type: application/json

{
  "action": "Delete",
  "mediaId": "media-001"
}
```

#### **Habilitar/Deshabilitar Imagen**
```http
PATCH /properties/{id}/media
Authorization: Bearer {accessToken}
Content-Type: application/json

{
  "action": "Toggle",
  "mediaId": "media-001"
}
```

### **4.3. Obtener Media de una Propiedad**
```http
GET /properties/{id}/media
```

**Respuesta:**
```json
{
  "cover": {
    "type": "Image",
    "url": "https://vercel-blob.com/properties/prop-001/cover.jpg",
    "index": 0
  },
  "featuredMedia": [
    {
      "id": "media-001",
      "type": "Image",
      "url": "https://vercel-blob.com/properties/prop-001/photo-01.jpg",
      "index": 1,
      "featured": true,
      "enabled": true
    }
  ],
  "allMedia": [
    {
      "id": "media-001",
      "type": "Image",
      "url": "https://vercel-blob.com/properties/prop-001/photo-01.jpg",
      "index": 1,
      "featured": true,
      "enabled": true
    }
  ],
  "totalCount": 1,
  "featuredCount": 1
}
```

---

## 🔍 **5. Búsqueda y Filtros**

### **Búsqueda Avanzada de Propiedades**
```http
GET /properties/advanced?city=Key Biscayne&minPrice=1000000&maxPrice=5000000&propertyType=Villa&hasPool=true&page=1&pageSize=20
```

**Parámetros Disponibles:**
- `city`: Ciudad de la propiedad
- `neighborhood`: Vecindario
- `propertyType`: Tipo de propiedad (Villa, Apartment, House)
- `minPrice`/`maxPrice`: Rango de precios
- `bedrooms`/`bathrooms`: Número de habitaciones/baños
- `hasPool`/`hasGarden`/`hasParking`: Amenidades
- `year`: Año de construcción
- `status`: Estado (Active, Sold, OffMarket)

---

## 📊 **6. Estadísticas del Usuario**

### **Obtener Estadísticas de Propiedades**
```http
GET /auth/owner/properties/stats
Authorization: Bearer {accessToken}
```

**Respuesta:**
```json
{
  "totalProperties": 15,
  "activeProperties": 12,
  "soldProperties": 2,
  "offMarketProperties": 1,
  "totalValue": 45000000,
  "averagePrice": 3000000,
  "propertiesByType": {
    "Villa": 8,
    "Apartment": 5,
    "House": 2
  },
  "propertiesByCity": {
    "Key Biscayne": 10,
    "Miami Beach": 5
  }
}
```

---

## 🚨 **7. Códigos de Error Comunes**

### **401 Unauthorized**
```json
{
  "type": "https://tools.ietf.org/html/rfc7235#section-3.1",
  "title": "Unauthorized",
  "status": 401,
  "detail": "Invalid or expired access token"
}
```

### **403 Forbidden**
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.3",
  "title": "Forbidden",
  "status": 403,
  "detail": "You don't have permission to access this resource"
}
```

### **404 Not Found**
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
  "title": "Not Found",
  "status": 404,
  "detail": "Property not found"
}
```

### **422 Validation Error**
```json
{
  "type": "https://tools.ietf.org/html/rfc4918#section-11.2",
  "title": "Validation Error",
  "status": 422,
  "detail": "Invalid input data",
  "errors": {
    "email": ["Invalid email format"],
    "price": ["Price must be greater than 0"]
  }
}
```

---

## 🔧 **8. Headers Requeridos**

### **Autenticación:**
```
Authorization: Bearer {accessToken}
```

### **Content-Type:**
```
Content-Type: application/json
```

### **CORS:**
```
Origin: http://localhost:3000
```

---

## 💻 **9. Ejemplo de Flujo Completo**

### **1. Login del Usuario**
```javascript
const loginResponse = await fetch('/auth/owner/login', {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify({ email, password })
});

const { accessToken } = await loginResponse.json();
```

### **2. Obtener Perfil**
```javascript
const profileResponse = await fetch('/auth/owner/profile', {
  headers: { 'Authorization': `Bearer ${accessToken}` }
});

const profile = await profileResponse.json();
```

### **3. Actualizar Imagen de Portada**
```javascript
const updateCoverResponse = await fetch(`/properties/${propertyId}/cover`, {
  method: 'PATCH',
  headers: {
    'Authorization': `Bearer ${accessToken}`,
    'Content-Type': 'application/json'
  },
  body: JSON.stringify({
    url: newCoverUrl,
    type: 'Image'
  })
});
```

### **4. Agregar Nueva Imagen a Galería**
```javascript
const addImageResponse = await fetch(`/properties/${propertyId}/media`, {
  method: 'PATCH',
  headers: {
    'Authorization': `Bearer ${accessToken}`,
    'Content-Type': 'application/json'
  },
  body: JSON.stringify({
    action: 'Add',
    media: {
      url: newImageUrl,
      type: 'Image',
      index: 5,
      featured: false
    }
  })
});
```

---

## 🎯 **10. Mejores Prácticas**

### **Gestión de Tokens:**
- Almacenar `accessToken` en memoria o localStorage
- Implementar refresh automático antes de expirar
- Manejar errores 401 con redirect a login

### **Gestión de Imágenes:**
- Validar formatos de imagen (JPG, PNG, WebP)
- Comprimir imágenes antes de subir
- Usar CDN para distribución global
- Implementar lazy loading en galerías

### **Validaciones:**
- Validar URLs de imágenes antes de enviar
- Verificar permisos antes de operaciones CRUD
- Implementar rate limiting para subidas
- Validar tamaños de archivo

### **UX:**
- Mostrar loading states durante operaciones
- Implementar drag & drop para galerías
- Preview de imágenes antes de confirmar
- Confirmación antes de eliminar

---

## 📝 **Notas Importantes**

- **Base URL**: `http://localhost:5208` (desarrollo) / `https://real-state.jsburbano.dev` (producción)
- **Rate Limiting**: Configurado para prevenir abuso de la API
- **CORS**: Configurado para `http://localhost:3000` y `https://real-state.jsburbano.dev`
- **Autenticación**: JWT con expiración de 1 hora
- **Formato de Respuesta**: JSON con RFC 7807 Problem Details para errores

---

## 🎯 **Estado de Implementación del Dashboard de Usuario**

### ✅ **100% IMPLEMENTADO Y FUNCIONANDO**

#### **🔐 Autenticación y Sesión (100%)**
- `POST /auth/owner/login` ✅
- `POST /auth/owner/refresh` ✅
- `POST /auth/owner/logout` ✅

#### **👤 Información del Usuario (100%)**
- `GET /auth/owner/profile` ✅
- `PUT /auth/owner/profile` ✅

#### **🏠 Gestión de Propiedades del Usuario (100%)**
- `GET /auth/owner/properties` ✅
- `GET /auth/owner/properties/stats` ✅

#### **🏠 Gestión de Propiedades (100%)**
- `GET /properties/{id}` ✅
- `GET /properties` ✅
- `PUT /properties/{id}` ✅
- `DELETE /properties/{id}` ✅

#### **🖼️ CRUD de Imágenes (100%)**
- `PATCH /properties/{id}/cover` ✅
- `PATCH /properties/{id}/gallery` ✅
- `PATCH /properties/{id}/media` ✅
- `GET /properties/{id}/media` ✅

#### **🔍 Búsqueda y Filtros (100%)**
- `GET /properties/advanced` ✅

#### **📊 Estadísticas del Usuario (100%)**
- `GET /auth/owner/properties/stats` ✅

### 🚀 **Características Implementadas:**

1. **Autenticación JWT completa** con refresh tokens
2. **Gestión de perfil de usuario** con validaciones
3. **Dashboard de propiedades** del usuario autenticado
4. **Estadísticas en tiempo real** de las propiedades
5. **CRUD completo de imágenes** con soporte para videos
6. **Búsqueda avanzada** con múltiples filtros
7. **Paginación** y ordenamiento
8. **Validaciones robustas** con FluentValidation
9. **Manejo de errores** con RFC 7807 Problem Details
10. **CORS configurado** para desarrollo y producción

### 🔧 **Archivos Creados/Modificados:**

- ✅ `UpdateOwnerProfileRequest.cs` - DTO para actualizar perfil
- ✅ `UpdateOwnerProfileRequestValidator.cs` - Validador con reglas
- ✅ `IAuthService.cs` - Interfaz extendida
- ✅ `AuthService.cs` - Implementación del servicio
- ✅ `IPropertyService.cs` - Interfaz extendida
- ✅ `PropertyService.cs` - Implementación del servicio
- ✅ `IPropertyRepository.cs` - Interfaz extendida
- ✅ `PropertyRepository.cs` - Implementación del repositorio
- ✅ `Program.cs` - Endpoints del dashboard implementados

### 🎉 **¡Dashboard de Usuario Completamente Implementado!**

El sistema está listo para ser usado en producción con todas las funcionalidades del dashboard de usuario implementadas y probadas.

---

## 🔗 **Enlaces Relacionados**

- [API Routes Documentation](./API_ROUTES.md)
- [API Integration Guide](./API_INTEGRATION_GUIDE.md)
- [Postman Collection](./Million-Properties-API.postman_collection.json)

---

*Documentación generada para Million Real Estate API v1.0*
