# Arquitectura

## Vista General
Clean Architecture con separación de capas y dependencias dirigidas hacia adentro:

- Domain (entidades): independiente de frameworks
- Application (casos de uso): orquesta lógica, define interfaces
- Infrastructure (adaptadores): MongoDB, repositorios, índices
- Web (framework): minimal API, DI, middlewares, Swagger

## Modelo de Datos
Colección `properties`:
```json
{ "Id": ObjectId, "IdOwner": string, "Name": string, "AddressProperty": string, "PriceProperty": decimal, "Image": string }
```

## Índices MongoDB
- Texto: `{ Name: "text", AddressProperty: "text" }`
- Numérico: `{ PriceProperty: 1 }`

## Búsqueda y Rendimiento
- `$text` cuando hay `name` y `address`; regex `i` si no
- Rango de precios `$gte/$lte`
- Proyección ligera y paginación+orden soportados

## ProblemDetails (RFC 7807)
Forma consistente:
```json
{ "type", "title", "status", "detail", "instance", "extensions": { "correlationId" } }
```
- 400 Validación, 404 NotFound, 429 RateLimit, 500 Error inesperado

## Correlation Id
- Header `X-Correlation-Id` aceptado y devuelto
- Incluido en `ProblemDetails.extensions`

## Logging (Serilog)
- JSON a consola
- Enriquecido con `correlationId`, ruta, método, estado, elapsedMs

## Seguridad y CORS
- Allowlist desde `CORS_ORIGINS`
- Límite simple por IP/minuto

## Pruebas
- NUnit; tests de validación, servicios y middlewares

## Despliegue
- Dockerfile para API
- `docker compose` con Mongo y Mongo Express