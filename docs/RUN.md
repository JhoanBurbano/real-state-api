# Guía Rápida: Ejecutar el Proyecto (Run Project)

Esta guía explica cómo ejecutar la API localmente y con Docker, crear índices y sembrar datos de ejemplo.

## Prerrequisitos
- .NET 8 SDK
- MongoDB (local) o Docker
- Opcional: mongosh y mongoimport para seed

## Variables de entorno
- MONGO_URI (default: mongodb://localhost:27017)
- MONGO_DB (default: million)
- CORS_ORIGINS (default: http://localhost:3000)
- RATE_LIMIT_PERMINUTE (default: 120)
- LOG_LEVEL (default: Information)

## Ejecutar localmente
1) Crear índices y seed (local):
```bash
mongosh "${MONGO_URI:-mongodb://localhost:27017}/${MONGO_DB:-million}" ops/db/create-indexes.js
mongoimport --uri "${MONGO_URI:-mongodb://localhost:27017}/${MONGO_DB:-million}" \
  --collection properties --file ops/db/properties.seed.json --jsonArray
```
2) Lanzar la API:
```bash
export ASPNETCORE_URLS=http://localhost:8080
export MONGO_URI=${MONGO_URI:-mongodb://localhost:27017}
export MONGO_DB=${MONGO_DB:-million}
export CORS_ORIGINS=${CORS_ORIGINS:-http://localhost:3000}
export RATE_LIMIT_PERMINUTE=${RATE_LIMIT_PERMINUTE:-120}
export LOG_LEVEL=${LOG_LEVEL:-Information}

dotnet build
dotnet run --project src/Million.Web/Million.Web.csproj
```
3) Swagger/UI: http://localhost:8080/swagger

### Endpoints clave
- GET /properties?name=&address=&minPrice=&maxPrice=&page=&pageSize=&sort=
- GET /properties/{id}
- Salud: GET /health/live, GET /health/ready

### Ejemplos
```bash
curl -s "http://localhost:8080/properties?name=miami&minPrice=1000000&maxPrice=5000000&sort=-price" \
  -H "X-Correlation-Id: demo-corr-id"
```

## Ejecutar con Docker
```bash
docker compose -f ops/docker-compose.yml up --build
```
- API: http://localhost:8080/swagger
- Mongo Express: http://localhost:8081

## Tests
```bash
dotnet test
```

## Solución de problemas
- 404 con ProblemDetails: revisa extensions.correlationId y logs.
- Límite de peticiones (429): espera 60s o ajusta RATE_LIMIT_PERMINUTE.
- CORS: añade tu origen a CORS_ORIGINS separado por comas.