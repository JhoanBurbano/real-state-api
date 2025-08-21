# Guía de Desarrollo (Onboarding)

## Objetivo
API de propiedades de lujo en .NET 8 + MongoDB con Clean Architecture, ProblemDetails, Serilog, Correlation-Id, rate limiting, CORS y Swagger.

## Requisitos
- .NET 8 SDK
- MongoDB (local/Docker)
- Editor/IDE: VS Code o Rider

## Estructura
- `src/Million.Domain`: Entidades puras
- `src/Million.Application`: DTOs, servicios, validaciones, interfaces
- `src/Million.Infrastructure`: MongoDB (contexto, repos), migraciones índices
- `src/Million.Web`: API, DI, middlewares, Swagger
- `tests/Million.Tests`: Unit tests (NUnit)

## Flujo de trabajo
1. Validar input (FluentValidation)
2. Servicio de aplicación construye caso de uso y llama repositorio
3. Repositorio ejecuta filtros/orden/paginación en Mongo
4. Web maneja errores con ProblemDetails y agrega correlationId

## Middlewares
- Correlation-Id: Lee `X-Correlation-Id` o genera GUID; propaga a respuesta y logs
- ProblemDetails: Mapea excepciones/validaciones a RFC7807
- Serilog: logging estructurado (JSON) a consola

## Validaciones
- `page >= 1`, `1 <= pageSize <= 100`
- `minPrice <= maxPrice`
- `sort` ∈ {price, -price, name, -name}
- Strings ≤ 200 chars

## Consultas (Repositorio)
- Si name y address: `$text` combinando ambos
- Si solo uno: regex case-insensitive
- Precio: `$gte`/`$lte`
- Orden: price/name asc/desc; default `-price`
- Paginación: `skip = (page-1)*pageSize`

## Endpoints
- GET `/properties` (paginado + filtros)
- GET `/properties/{id}` (404 con ProblemDetails si no existe)

## Ejecutar
- Ver `docs/RUN.md`

## Estilo de código
- Nombres descriptivos, early returns, manejo explícito de errores, sin comentarios triviales

## Testing
```bash
dotnet test
```
- Cobertura objetivo: ≥80% en Application

## Datos & Índices
- `ops/db/create-indexes.js`: índice de texto (Name, AddressProperty) y numérico (PriceProperty)
- `ops/db/properties.seed.json`: ~25 propiedades de muestra

## Observabilidad
- Logs con `correlationId`, `method`, `path`, `status`, `elapsedMs`

## CORS y Rate Limiting
- CORS: `CORS_ORIGINS` separado por comas
- Rate limit simple por IP/minuto: `RATE_LIMIT_PERMINUTE`