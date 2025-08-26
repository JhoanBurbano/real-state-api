# 🗄️ **Base de Datos - Seeds y Migraciones**

Este directorio contiene todos los archivos necesarios para inicializar y poblar la base de datos MongoDB de Million Real Estate API.

## 📁 **Archivos Disponibles**

### **Seeds JSON (Para importación manual)**
- **`properties.seed.json`** - Datos de ejemplo para propiedades
- **`owners.seed.json`** - Datos de ejemplo para propietarios
- **`owner-sessions.seed.json`** - Datos de ejemplo para sesiones de propietarios

### **Scripts de MongoDB**
- **`create-indexes.js`** - Crea todos los índices necesarios
- **`seed-database.js`** - Script completo para poblar la base de datos

## 🚀 **Uso Rápido**

### **Opción 1: Script Completo (Recomendado)**
```bash
# Ejecutar el script completo de seed
mongosh "mongodb://localhost:27017/million" ops/db/seed-database.js
```

### **Opción 2: Solo Índices**
```bash
# Crear solo los índices
mongosh "mongodb://localhost:27017/million" ops/db/create-indexes.js
```

### **Opción 3: Importación Manual**
```bash
# Importar datos manualmente
mongoimport --db million --collection properties --file ops/db/properties.seed.json
mongoimport --db million --collection owners --file ops/db/owners.seed.json
mongoimport --db million --collection owner_sessions --file ops/db/owner-sessions.seed.json
```

## 🏗️ **Estructura de los Datos**

### **Properties (Propiedades)**
```json
{
  "id": "prop-001",
  "ownerId": "owner-001",
  "name": "Oceanfront Villa",
  "address": "100 Ocean Dr, Miami Beach, FL",
  "price": 12500000,
  "codeInternal": "MB001",
  "year": 2020,
  "status": "Active",
  "cover": { /* Sistema de media nuevo */ },
  "media": [ /* Galería de imágenes */ ],
  "description": "Luxurious oceanfront villa...",
  "city": "Miami Beach",
  "neighborhood": "South Beach",
  "propertyType": "Villa",
  "size": 8500,
  "bedrooms": 5,
  "bathrooms": 6,
  "hasPool": true,
  "hasGarden": true,
  "hasParking": true,
  "isFurnished": true,
  "availableFrom": "2024-01-15T00:00:00Z",
  "availableTo": "2024-12-31T00:00:00Z",
  "traces": [ /* Historial de ventas */ ],
  "coverImage": "https://...", /* Campo legacy */
  "images": [ /* Campo legacy */ ],
  "createdAt": "2024-01-01T00:00:00Z",
  "updatedAt": "2024-01-01T00:00:00Z",
  "isActive": true
}
```

### **Owners (Propietarios)**
```json
{
  "id": "owner-001",
  "fullName": "Carlos Rodriguez",
  "email": "carlos.rodriguez@million.com",
  "phoneE164": "+13055551234",
  "photoUrl": "https://...",
  "role": "Owner",
  "isActive": true,
  "passwordHash": "$argon2id$...",
  "createdAt": "2024-01-01T00:00:00Z",
  "updatedAt": "2024-01-01T00:00:00Z"
}
```

### **Owner Sessions (Sesiones)**
```json
{
  "id": "session-001",
  "ownerId": "owner-001",
  "refreshTokenHash": "$argon2id$...",
  "ip": "192.168.1.100",
  "userAgent": "Mozilla/5.0...",
  "issuedAt": "2024-01-01T00:00:00Z",
  "expiresAt": "2024-12-31T23:59:59Z",
  "revokedAt": null,
  "rotatedAt": null
}
```

## 🔍 **Índices Creados**

### **Properties**
- `idx_text_name_address` - Búsqueda de texto en nombre y dirección
- `idx_price` - Ordenamiento por precio
- `idx_code_internal` - Código interno único
- `idx_owner_id` - Propietario
- `idx_status` - Estado de la propiedad
- `idx_city` - Ciudad
- `idx_neighborhood` - Vecindario
- `idx_property_type` - Tipo de propiedad
- `idx_bedrooms` - Habitaciones
- `idx_bathrooms` - Baños
- `idx_year` - Año de construcción
- `idx_is_active` - Propiedades activas
- `idx_created_at` - Fecha de creación

### **Owners**
- `idx_email` - Email único
- `idx_role` - Rol del propietario
- `idx_owner_is_active` - Propietarios activos
- `idx_owner_created_at` - Fecha de creación

### **Owner Sessions**
- `idx_session_owner_id` - Propietario de la sesión
- `idx_refresh_token_hash` - Hash del token de refresco
- `idx_expires_at` - Expiración (con TTL)
- `idx_revoked_at` - Sesiones revocadas

## 🔄 **Compatibilidad Legacy**

Los datos incluyen tanto la nueva estructura de media (`cover`, `media`) como los campos legacy (`coverImage`, `images`) para mantener compatibilidad con versiones anteriores.

## 📊 **Datos de Ejemplo Incluidos**

- **3 Propiedades** con diferentes características y ubicaciones
- **4 Propietarios** (3 owners + 1 admin)
- **3 Sesiones** de ejemplo para testing
- **URLs de Vercel Blob** para imágenes de ejemplo

## ⚠️ **Notas Importantes**

1. **Passwords**: Los hashes de contraseña son de ejemplo (`$argon2id$v=19$m=65536,t=3,p=1$dGVzdA$test`)
2. **URLs**: Las URLs de imágenes son ejemplos y no funcionarán en producción
3. **IDs**: Los IDs son predefinidos para facilitar las relaciones entre entidades
4. **Timestamps**: Todas las fechas están en UTC

## 🧪 **Testing**

Para testing, puedes usar las credenciales:
- **Admin**: `admin@million.com` / `Admin123!`
- **Owner**: `carlos.rodriguez@million.com` / `Password123!`

