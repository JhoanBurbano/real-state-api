ROLE
You are my Senior Back-End Engineer (C#/.NET 8/9) + Tech Writer pair-programmer.
Deliver a production-grade API for a luxury real estate demo using .NET + MongoDB with Clean Architecture, RFC 7807 ProblemDetails, structured logging (Serilog), Correlation-Id propagation, rate limiting, CORS allowlist, Swagger, NUnit tests, seed + indexes, Docker, and first-class DX docs.

FIRST, WRITE AN ENGLISH EXECUTION PLAN
- Outline architecture, folders, key files, dependencies, risks, environment variables, and run commands.
- <= 200 lines. After printing the plan, implement everything.

REQUIREMENTS (from the technical test) — API scope
- Tech: .NET 8 or 9 (C# 12), MongoDB. Tests with NUnit.
- List properties with filters by name, address, and price range.
- DTO fields: IdOwner, Name, AddressProperty, PriceProperty, Image (one URL).
- Clean architecture, good error handling, optimized queries, documentation, performance for multiple filters, and steps to run (including DB seed/backup).
- Deliverables must be runnable locally and via Docker.

ACCEPTANCE CRITERIA
1) Endpoints
   - GET /properties?name=&address=&minPrice=&maxPrice=&page=&pageSize=&sort=
     • Filters: name/address (case-insensitive contains or text search), minPrice/maxPrice (inclusive range)
     • Pagination: page>=1; pageSize default=12, max=100
     • Sorting: sort ∈ {price,-price,name,-name}; default=-price
   - GET /properties/{id}  → 404 as RFC 7807 ProblemDetails if not found
   - (Optional, feature-flagged & disabled by default) POST/PUT/DELETE /properties

2) Data Model (MongoDB)
   - Collection: properties
   - Document: { Id (ObjectId), IdOwner: string, Name: string, AddressProperty: string, PriceProperty: decimal/number, Image: string }

3) Indexes (Performance)
   - Compound text index: { Name, AddressProperty }
   - Numeric index: { PriceProperty: 1 }

4) Clean Architecture (.NET)
   - Projects:
     /src/Million.Domain           (entities, contracts)
     /src/Million.Application      (DTOs, services, validators, common types)
     /src/Million.Infrastructure   (Mongo, repositories, index init)
     /src/Million.Web              (minimal API/controllers, DI, middlewares, Swagger)
     /tests/Million.Tests          (NUnit unit tests + optional integration fixtures)

5) Reliability, Security & Observability
   - RFC 7807 ProblemDetails everywhere (400/404/422/429/500) with consistent shape:
     { type, title, status, detail, instance, extensions: { correlationId } }
   - Correlation-Id middleware:
     • Accept/propagate X-Correlation-Id from request; if absent, generate a GUID.
     • Attach to HttpContext.Items and Serilog log scope; include in responses (ProblemDetails.extensions.correlationId and response header).
   - Structured logging (Serilog) in JSON, with enrichers: correlationId, requestPath, method, statusCode, elapsedMs.
   - Basic IP rate limiting (e.g., fixed or sliding window) returning 429 as ProblemDetails.
   - CORS allowlist via ENV.
   - Health endpoints: /health/live and /health/ready (200 OK with simple payload).

6) Documentation & DX
   - Swagger/OpenAPI (group “Properties”), with examples and response schemas.
   - Postman/Insomnia collection in /docs.
   - README with: prerequisites, ENV, run (dev & docker), tests, seed, index creation, and sample queries.
   - ADR (short) for: Mongo indexes, pagination/sorting policy, ProblemDetails strategy, Correlation-Id choice.

7) Testing (NUnit)
   - Coverage target: ≥80% in Application layer.
   - Unit tests:
     • Query validation (page/pageSize bounds; minPrice<=maxPrice; sort mapping)
     • Repository filter builder (name/address/price permutations; regex vs $text fallback)
     • Services: pagination math, projection, not-found returns 404 ProblemDetails
     • Middlewares: ensure correlation id presence; ProblemDetails shape for 400/404 and unexpected exceptions (500)
   - Optional integration test with ephemeral Mongo: create indexes, insert few docs, assert query correctness & performance.

8) Seed & Index Scripts
   - /ops/db/properties.seed.json  (~25 items; one Image each; realistic South Florida areas)
   - /ops/db/create-indexes.js     (compound text index + price index)
   - Seed commands in README (mongosh + mongoimport)

9) Docker
   - /ops/docker-compose.yml: services mongo, mongo-express (optional), api
   - api depends_on mongo with healthcheck; expose ports; ENV wired

10) Delivery Summary (print at end)
   - HOW TO RUN (local & docker), Swagger URL, example queries, “dotnet test” summary, and index evidence

ENVIRONMENT VARIABLES
- ASPNETCORE_ENVIRONMENT=Development|Production
- MONGO_URI=mongodb://localhost:27017
- MONGO_DB=million
- CORS_ORIGINS=http://localhost:3000
- RATE_LIMIT_PERMINUTE=120
- LOG_LEVEL=Information

IMPLEMENTATION DETAILS

A) Domain & DTOs
- Entity: Property { Id, IdOwner, Name, AddressProperty, PriceProperty, Image }
- DTO (API): PropertyDto { Id, IdOwner, Name, AddressProperty, PriceProperty, Image }
- Query DTO: PropertyListQuery { name?, address?, minPrice?, maxPrice?, page=1, pageSize=12, sort? }

B) Validation (FluentValidation or minimal guards)
- page ≥ 1; 1 ≤ pageSize ≤ 100; if minPrice & maxPrice → min ≤ max; sort in allowed set; strings length ≤ 200

C) Repository (MongoDB.Driver)
- Build filter:
  • If both name/address are provided, prefer $text when possible; else case-insensitive regex for contains
  • Price range via $gte/$lte
- Build sort by “price” or “name”, supporting ascending/descending
- Projection: only required fields (no heavy blobs)
- Ensure indexes at startup (idempotent)

D) Web (Minimal API or Controllers)
- Middlewares:
  • CorrelationIdMiddleware: read "X-Correlation-Id" → set in HttpContext.Items + Response headers
  • Serilog request logging (log each request with correlationId and elapsedMs)
  • Global ExceptionHandler → return ProblemDetails with correlationId and type/title per exception
  • ModelState → 400 ProblemDetails with validation errors
- Endpoints:
  • GET /properties → returns PagedResult<PropertyDto> { items, total, page, pageSize }
  • GET /properties/{id} → 200 or 404 ProblemDetails
- Swagger: tag=Properties; examples for querystrings
- CORS: allowlist from ENV
- Rate limiting: per IP; on breach return 429 ProblemDetails with retryAfter (if feasible)

E) ProblemDetails (RFC 7807)
- Map known errors:
  • 400 Validation: title="Validation Failed", type="https://example.com/problems/validation-error"
  • 404 NotFound: title="Resource Not Found", type="https://example.com/problems/not-found"
  • 422 Unprocessable (if needed)
  • 429 Too Many Requests: include "retryAfter" hint
  • 500 Unexpected: title="Unexpected Error"
- Always include extensions.correlationId and instance=requestPath

F) Serilog (structured logs)
- Serilog configuration in Program.cs or appsettings:
  • Enrich: FromLogContext + correlationId + Environment + Application
  • OutputTemplate JSON
- Sample log shape:
  { ts, level, correlationId, method, path, status, elapsedMs, message }

G) Tests (NUnit)
- PropertyListQueryValidatorTests.cs
- PropertyRepositoryFilterBuilderTests.cs
- PropertyServiceTests.cs (happy paths + edges)
- ProblemDetailsMiddlewareTests.cs (maps exceptions and validation to RFC 7807, includes correlationId)
- CorrelationIdMiddlewareTests.cs (incoming/outgoing header behavior)

H) Output Artifacts (create and implement real code)
- /src/Million.Domain/Entities/Property.cs
- /src/Million.Application/DTOs/{PropertyDto.cs,PropertyListQuery.cs}
- /src/Million.Application/Common/PagedResult.cs
- /src/Million.Application/Interfaces/IPropertyService.cs
- /src/Million.Application/Services/PropertyService.cs
- /src/Million.Application/Validation/PropertyListQueryValidator.cs
- /src/Million.Infrastructure/Config/MongoOptions.cs
- /src/Million.Infrastructure/Persistence/MongoContext.cs
- /src/Million.Infrastructure/Repositories/{IPropertyRepository.cs,PropertyRepository.cs}
- /src/Million.Infrastructure/Migrations/EnsureIndexes.cs
- /src/Million.Web/Program.cs
- /src/Million.Web/Middlewares/{CorrelationIdMiddleware.cs,ProblemDetailsMiddleware.cs}
- /src/Million.Web/Controllers/PropertiesController.cs  (or minimal endpoints in Program.cs)
- /src/Million.Web/appsettings.json
- /tests/Million.Tests/{PropertyListQueryValidatorTests.cs,PropertyServiceTests.cs,PropertyRepositoryFilterBuilderTests.cs,ProblemDetailsMiddlewareTests.cs,CorrelationIdMiddlewareTests.cs}
- /ops/db/properties.seed.json
- /ops/db/create-indexes.js
- /ops/docker-compose.yml
- /docs/API.postman_collection.json
- /docs/adr/001-architecture.md
- /README.md

I) Commands (scaffold & run)
- dotnet new sln -n Million
- dotnet new classlib -n Million.Domain -o src/Million.Domain
- dotnet new classlib -n Million.Application -o src/Million.Application
- dotnet new classlib -n Million.Infrastructure -o src/Million.Infrastructure
- dotnet new webapi   -n Million.Web -o src/Million.Web
- dotnet new nunit    -n Million.Tests -o tests/Million.Tests
- dotnet sln add src/**/**.csproj tests/**/**.csproj
- dotnet add src/Million.Web package Swashbuckle.AspNetCore Serilog.AspNetCore Serilog.Sinks.Console
- dotnet add src/Million.Infrastructure package MongoDB.Driver
- dotnet add tests/Million.Tests package NSubstitute FluentAssertions
- dotnet build && dotnet test
- Seed (local):
  mongosh "mongodb://localhost:27017/million" ops/db/create-indexes.js
  mongoimport --uri "mongodb://localhost:27017/million" --collection properties --file ops/db/properties.seed.json --jsonArray
- Docker:
  docker compose -f ops/docker-compose.yml up --build

WHAT TO OUTPUT NOW
1) The Execution Plan (EN).
2) The full folder structure and all code files implemented as specified (including middlewares for ProblemDetails & CorrelationId).
3) Swagger ready + Postman collection in /docs.
4) DB seed + indexes scripts, Docker Compose, and README with precise run steps.
5) Verification section showing:
   • Example GET /properties queries and sample JSON response
   • Evidence of indexes (output from ensure indexes)
   • dotnet test summary
   • A sample ProblemDetails (404) including extensions.correlationId
   • A sample Serilog JSON log line with correlationId
   • Packaging guidance (zip/repo)

Inlclude full sagger (openapi) endpoints documentation and full errro handling
