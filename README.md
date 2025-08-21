# Million Properties API

Luxury real estate demo API using .NET 8 + MongoDB with Clean Architecture, RFC 7807, Serilog, Correlation-Id, rate limiting, CORS, Swagger, NUnit, seed + indexes, and Docker.

## Projects
- `src/Million.Domain`: Entities
- `src/Million.Application`: DTOs, services, validation, interfaces
- `src/Million.Infrastructure`: Mongo context, repository, indexes
- `src/Million.Web`: Web API, middleware, DI, Swagger
- `tests/Million.Tests`: NUnit tests

## Environment Variables
- `ASPNETCORE_ENVIRONMENT=Development|Production`
- `MONGO_URI=mongodb://localhost:27017`
- `MONGO_DB=million`
- `CORS_ORIGINS=http://localhost:3000`
- `RATE_LIMIT_PERMINUTE=120`
- `LOG_LEVEL=Information`

## Run locally
```
export MONGO_URI=mongodb://localhost:27017
export MONGO_DB=million
export CORS_ORIGINS=http://localhost:3000
export RATE_LIMIT_PERMINUTE=120
export LOG_LEVEL=Information

# create indexes and seed
mongosh "${MONGO_URI}/${MONGO_DB}" ops/db/create-indexes.js
mongoimport --uri "${MONGO_URI}/${MONGO_DB}" --collection properties --file ops/db/properties.seed.json --jsonArray

# run API
export ASPNETCORE_URLS=http://localhost:8080
dotnet build
dotnet run --project src/Million.Web/Million.Web.csproj
```

Swagger: http://localhost:8080/swagger

## Docker
```
docker compose -f ops/docker-compose.yml up --build
```

## Endpoints
- `GET /properties?name=&address=&minPrice=&maxPrice=&page=&pageSize=&sort=`
- `GET /properties/{id}`
- `GET /health/live`, `GET /health/ready`

## Example queries
- `GET /properties?name=miami&minPrice=1000000&maxPrice=5000000&page=1&pageSize=12&sort=-price`

## ProblemDetails example (404)
```json
{
  "type": "https://example.com/problems/not-found",
  "title": "Resource Not Found",
  "status": 404,
  "detail": "Property 000000000000000000000000 not found",
  "instance": "/properties/000000000000000000000000",
  "extensions": { "correlationId": "guid-here" }
}
```

## Sample Serilog JSON
```json
{ "ts": "2025-08-21T12:00:00Z", "level": "Information", "message": "HTTP GET /properties responded 200 in 25 ms", "correlationId": "guid-here" }
```

## Tests
```
dotnet test
```

## Seed & Index evidence
Indexes created via `EnsureIndexes` at startup and via `ops/db/create-indexes.js`.

## Packaging
- Zip the repo directory or push to a Git repository.

