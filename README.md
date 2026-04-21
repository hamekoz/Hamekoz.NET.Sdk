# Hamekoz.NET.Sdk
SDK para .NET 9 orientado a construir APIs REST con un enfoque **template/reutilizable** y bajo coste de desarrollo para aplicaciones específicas.
.NET 9 SDK for building REST APIs with a reusable template-based approach and low development cost for specific applications.

## Objetivo

`Hamekoz.NET.Sdk` encapsula patrones repetitivos de CRUD, controladores, DTOs, middleware y registro de dependencias para que cada aplicación de negocio:

- escriba menos código boilerplate,
- mantenga consistencia arquitectónica,
- y extienda comportamiento solo donde aporta valor.

## Estructura del repositorio

| Proyecto | Rol |
| --- | --- |
| `Hamekoz.Api` | Núcleo del SDK: `Entity`, `CrudService`, `CrudController`, excepciones, middlewares y extensiones DI |
| `Hamekoz.Api.Example` | Aplicación de ejemplo para validar integración y flujo de uso |
| `Hamekoz.Api.Tests` | Pruebas unitarias del comportamiento base del SDK |
| `Hamekoz.NET.Sdk.AppHost` | Orquestación local con .NET Aspire |
| `Hamekoz.NET.Sdk.ServiceDefaults` | Defaults de observabilidad, health checks y resiliencia para Aspire |

## Arquitectura de componentes reutilizables

### 1) Modelo base de dominio

- `Entity` define `Id` como identidad común.
- `AuditableEntity` añade trazabilidad de creación/actualización.
- `ISoftDeletable` habilita borrado lógico sin duplicar lógica por entidad.

### 2) Capa de servicios template

- `ICrudService<TEntity>` define contrato estándar.
- `CrudService<TEntity, TDbContext>` implementa:
  - listado completo,
  - paginado (`PagedResult<T>`),
  - lectura por id,
  - creación,
  - actualización,
  - borrado físico o lógico según capacidades de la entidad.

La recomendación es **heredar** de `CrudService` solo cuando haya reglas de negocio específicas.

### 3) Capa de API template

- `CrudController<TEntity, ...>` expone endpoints REST estándar:
  - `GET /api/{controller}`
  - `GET /api/{controller}/paged`
  - `GET /api/{controller}/{id}`
  - `POST /api/{controller}`
  - `PUT /api/{controller}/{id}`
  - `DELETE /api/{controller}/{id}`
- También existe sobrecarga simplificada `CrudController<TEntity>` para escenarios con DTO único.

### 4) Contratos DTO

Marcadores para desacoplar operaciones y payloads:

- `IListItemDto`
- `IDetailDto`
- `ICreateDto`
- `IUpdateDto`
- `IFullDto` (composición de todos)

### 5) Errores consistentes

- Excepciones esperadas: `ValidationException` y `NotFoundException`.
- `ExceptionHandlingMiddleware` transforma errores a `ProblemDetails` (`application/problem+json`) y evita exponer detalles internos en errores 500.

### 6) Registro automático y extensibilidad por convención

Extensiones DI en `ServiceCollectionExtensions`:

- `AddHamekozApi<TDbContext>()` registra servicios CRUD para entidades detectadas por reflexión.
- `AddCrudServices<TDbContext>()` permite control más explícito del registro template.
- `AddUniqueImplementationOfServices()` registra interfaces con implementación única.
- `AddTemplateServices(...)` habilita patrones genéricos por herencia.

## Flujo recomendado para crear una app basada en el SDK

1. Definir entidades que hereden de `Entity`.
2. Declarar `DbSet<TEntity>` en el `DbContext`.
3. Crear DTOs con interfaces marcador adecuadas.
4. Configurar mapping en AutoMapper (`Profile`).
5. Crear controlador heredando `CrudController<...>`.
6. Registrar SDK con `services.AddHamekozApi<TDbContext>()`.
7. Activar middleware con `app.UseHamekozMiddlewares()`.

## Convenciones

- `net9.0`
- nullable enabled
- file-scoped namespaces
- nombres en `PascalCase` / `camelCase` según `.editorconfig`
- llaves obligatorias en estructuras de control
- commits con Conventional Commits

## Build y test

```bash
dotnet restore
dotnet build
dotnet test
```

## Principios para mantener bajo coste de desarrollo

1. **Convención sobre configuración**: usar registros automáticos por reflexión cuando sea posible.
2. **Extender en puntos concretos**: heredar servicios/controladores base en lugar de reescribir.
3. **Contratos estables**: mantener marker interfaces y `PagedResult<T>` como contratos reutilizables.
4. **Errores homogéneos**: centralizar manejo de excepciones con middleware.
5. **Ejemplo ejecutable siempre al día**: usar `Hamekoz.Api.Example` como plantilla viva para nuevos proyectos.

SDK base para construir servicios reutilizables del ecosistema Hamekoz sobre .NET.

## Proyectos principales

- Hamekoz.Api: infraestructura generica para CRUD y APIs REST.
- Hamekoz.Features: resolucion de features por aplicacion, plan y usuario.
- Hamekoz.Subscriptions: catalogo y ciclo de vida de suscripciones con plan free obligatorio.
- Hamekoz.Api.Tests: pruebas unitarias de la base comun.

## Nuevas capacidades de plataforma

- Cada aplicacion puede definir sus propios planes y features sin cambiar el dominio comun.
- Siempre existe un plan gratuito activo y por defecto para cada aplicacion.
- Cada usuario obtiene una suscripcion activa aunque no tenga plan pago.
- Los features efectivos se resuelven en capas: aplicacion, plan y usuario.

## Direccion de arquitectura

La base actual deja preparados dos dominios que luego pueden exponerse como microservicios del ecosistema:

- servicio de usuarios e identidad compartido
- servicio de suscripciones
- servicio de features

Mas detalle en docs/platform-services.md.
