# ADR 001: Architecture, Indexes, Pagination, and Error Strategy

Context: Build a demo luxury real estate API on .NET 8 + MongoDB with Clean Architecture.

Decision:
- Clean Architecture with Domain, Application, Infrastructure, Web projects.
- MongoDB text index on `Name` and `AddressProperty`, numeric index on `PriceProperty`.
- Pagination defaults: `page=1`, `pageSize=12`, max `pageSize=100`. Sorting by `price` or `name`, with `-` for desc; default `-price`.
- RFC7807 ProblemDetails everywhere, with `extensions.correlationId`. Correlation ID via `X-Correlation-Id` header or generated.

Consequences:
- Indexes accelerate multi-field queries; text index used when both name and address present; regex fallback otherwise.
- Predictable, consistent error payloads for diagnostics.
- Middleware keeps correlation id across logs and responses.

