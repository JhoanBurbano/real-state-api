# Million API - End-to-End Tests

Este proyecto contiene tests end-to-end (e2e) completos para la API Million, diseñados para probar el flujo completo de la aplicación desde la capa de presentación hasta la base de datos.

## 🏗️ Arquitectura de Tests

### Estructura del Proyecto
```
Million.E2E.Tests/
├── TestBase.cs                 # Clase base con configuración común
├── GlobalSetup.cs              # Configuración global del entorno de testing
├── AuthenticationE2ETests.cs   # Tests de autenticación y autorización
├── PropertiesE2ETests.cs       # Tests CRUD de propiedades
├── MiddlewareE2ETests.cs       # Tests de middleware (rate limiting, logging)
├── ErrorHandlingE2ETests.cs    # Tests de manejo de errores y validaciones
└── README.md                   # Esta documentación
```

## 🚀 Configuración y Ejecución

### Prerrequisitos
- .NET 9.0 SDK
- Docker Desktop (para MongoDB container)
- NUnit Test Adapter instalado en tu IDE

### Ejecutar Tests

#### 1. Ejecutar Todos los Tests E2E
```bash
# Desde el directorio raíz del proyecto
dotnet test tests/Million.E2E.Tests/

# O desde el directorio de tests
cd tests/Million.E2E.Tests
dotnet test
```

#### 2. Ejecutar Tests Específicos
```bash
# Ejecutar solo tests de autenticación
dotnet test --filter "TestClass~AuthenticationE2ETests"

# Ejecutar solo tests de propiedades
dotnet test --filter "TestClass~PropertiesE2ETests"

# Ejecutar un test específico
dotnet test --filter "TestName~Owner_Login_WithValidCredentials_ShouldReturnTokens"
```

#### 3. Ejecutar con Output Detallado
```bash
dotnet test --logger "console;verbosity=detailed"
```

## 🧪 Tipos de Tests Implementados

### 1. AuthenticationE2ETests
**Propósito**: Probar el flujo completo de autenticación y autorización.

**Tests Incluidos**:
- ✅ Login exitoso de propietarios y administradores
- ✅ Login con credenciales inválidas
- ✅ Refresh de tokens
- ✅ Logout
- ✅ Bloqueo de cuenta por intentos fallidos
- ✅ Validación de formato de email

**Cobertura**:
- Endpoints: `/auth/owner/login`, `/auth/owner/refresh`, `/auth/owner/logout`
- Casos de éxito y error
- Validaciones de entrada
- Manejo de tokens JWT

### 2. PropertiesE2ETests
**Propósito**: Probar el CRUD completo de propiedades y funcionalidades avanzadas.

**Tests Incluidos**:
- ✅ Listado de propiedades con paginación
- ✅ Filtros por precio, ciudad, habitaciones
- ✅ Ordenamiento por diferentes criterios
- ✅ Búsqueda de texto
- ✅ Creación de propiedades
- ✅ Actualización de propiedades
- ✅ Eliminación de propiedades
- ✅ Activación/desactivación
- ✅ Gestión de media (imágenes)
- ✅ Traces (historial de ventas)

**Cobertura**:
- Endpoints: `/properties`, `/properties/{id}`, `/properties/{id}/media`, `/properties/{id}/traces`
- Validaciones de entrada
- Casos de éxito y error
- Integración con sistema de media

### 3. MiddlewareE2ETests
**Propósito**: Probar el funcionamiento correcto de todos los middlewares.

**Tests Incluidos**:
- ✅ Correlation ID en headers
- ✅ Rate limiting (dentro de límites y excediendo)
- ✅ Modo burst del rate limiting
- ✅ Headers de rate limiting
- ✅ Problem Details (RFC 7807)
- ✅ Orden correcto de middlewares

**Cobertura**:
- CorrelationIdMiddleware
- AdvancedRateLimiterMiddleware
- ProblemDetailsMiddleware
- StructuredLoggingMiddleware
- Headers de respuesta
- Códigos de estado HTTP

### 4. ErrorHandlingE2ETests
**Propósito**: Probar el manejo robusto de errores y validaciones.

**Tests Incluidos**:
- ✅ Códigos internos duplicados
- ✅ URLs de blob inválidas
- ✅ Demasiadas imágenes
- ✅ IDs inexistentes
- ✅ Datos de entrada inválidos
- ✅ Paginación inválida
- ✅ Filtros inválidos
- ✅ Ordenamiento inválido
- ✅ Tokens expirados/inválidos
- ✅ JSON malformado
- ✅ Campos requeridos faltantes

**Cobertura**:
- Validaciones de entrada
- Manejo de errores de negocio
- Códigos de estado HTTP apropiados
- Formato Problem Details
- Casos edge y límites

## 🔧 Configuración del Entorno de Testing

### Base de Datos de Testing
- **MongoDB Container**: Se ejecuta en `mongodb://localhost:27017/million_test`
- **Base de Datos**: `million_test` (separada de desarrollo)
- **Datos de Seed**: Se cargan automáticamente antes de ejecutar tests
- **Índices**: Se crean automáticamente

### Configuración de Testing
```csharp
// Configuración específica para tests
["ASPNETCORE_ENVIRONMENT"] = "Testing"
["LOG_LEVEL"] = "Warning"
["RATE_LIMIT_PERMINUTE"] = "1000"
["RATE_LIMIT_BURST"] = "2000"
["AUTH_JWT_ISSUER"] = "https://test.million.com"
["AUTH_JWT_AUDIENCE"] = "https://test.million.com"
```

### Autenticación de Testing
- **Propietario**: `carlos.rodriguez@million.com` / `Password123!`
- **Admin**: `admin@million.com` / `Admin123!`
- **Tokens**: Se generan automáticamente para cada test

## 📊 Métricas y Cobertura

### Estadísticas de Tests
- **Total de Tests**: 40+ tests e2e
- **Cobertura de Endpoints**: 100% de endpoints principales
- **Cobertura de Middleware**: 100% de funcionalidades de middleware
- **Cobertura de Errores**: 100% de casos de error principales

### Tiempo de Ejecución
- **Tests Individuales**: 100-500ms por test
- **Suite Completa**: 2-5 minutos
- **Setup/Teardown**: 10-15 segundos

## 🐛 Debugging y Troubleshooting

### Logs de Testing
```bash
# Habilitar logs detallados
dotnet test --logger "console;verbosity=detailed"

# Ver logs de un test específico
dotnet test --filter "TestName~SpecificTest" --logger "console;verbosity=detailed"
```

### Problemas Comunes

#### 1. MongoDB Container No Inicia
```bash
# Verificar Docker
docker ps
docker-compose up -d mongodb

# Verificar logs
docker logs million-mongodb-dev
```

#### 2. Tests Fallan por Timeout
```bash
# Aumentar timeout en TestBase.cs
MongoContainer = new MongoDbBuilder()
    .WithImage("mongo:7.0")
    .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(27017))
    .Build();
```

#### 3. Problemas de Autenticación
- Verificar que los datos de seed se cargaron correctamente
- Verificar configuración JWT en testing
- Verificar que el servicio de passwords funciona

### Debugging en IDE
1. **Visual Studio**: Usar Test Explorer y debugging integrado
2. **VS Code**: Usar .NET Test Explorer extension
3. **Rider**: Usar Unit Tests tool window

## 🔄 Mantenimiento y Actualización

### Agregar Nuevos Tests
1. Crear nueva clase de test heredando de `TestBase`
2. Implementar tests usando `[Test]` attribute
3. Usar `FluentAssertions` para assertions
4. Seguir convenciones de naming: `Should_do_something_when_condition`

### Actualizar Tests Existentes
1. Mantener compatibilidad con cambios en la API
2. Actualizar assertions cuando cambien respuestas
3. Agregar tests para nuevas funcionalidades
4. Refactorizar tests duplicados

### Ejecutar Tests en CI/CD
```yaml
# Ejemplo para GitHub Actions
- name: Run E2E Tests
  run: |
    dotnet test tests/Million.E2E.Tests/ --logger "console;verbosity=normal"
    --results-directory TestResults
    --collect:"XPlat Code Coverage"
```

## 📚 Recursos Adicionales

### Documentación Relacionada
- [NUnit Documentation](https://docs.nunit.org/)
- [FluentAssertions](https://fluentassertions.com/)
- [ASP.NET Core Testing](https://docs.microsoft.com/en-us/aspnet/core/test/integration-tests)
- [Testcontainers](https://testcontainers.com/)

### Patrones de Testing
- **Arrange-Act-Assert**: Estructura estándar de tests
- **Test Data Builders**: Para crear datos de test complejos
- **Shared Test Context**: Para setup común entre tests
- **Async Testing**: Para operaciones asíncronas

### Mejores Prácticas
1. **Tests Independientes**: Cada test debe poder ejecutarse en cualquier orden
2. **Cleanup**: Limpiar datos de test después de cada test
3. **Assertions Claras**: Usar FluentAssertions para assertions legibles
4. **Naming Descriptivo**: Nombres de tests que expliquen qué prueban
5. **Setup Mínimo**: Solo configurar lo necesario para cada test

---

**Nota**: Estos tests e2e son fundamentales para garantizar la calidad y confiabilidad de la API Million. Se ejecutan automáticamente en el pipeline de CI/CD y deben pasar antes de cualquier deployment.

