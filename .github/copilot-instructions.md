# Copilot Instructions for Hamekoz.NET.Sdk

## Project Overview

**Hamekoz.NET.Sdk** is a .NET 9 SDK library that provides reusable generic CRUD (Create, Read, Update, Delete) infrastructure for building RESTful APIs with ASP.NET Core. Its goal is to reduce boilerplate by offering base classes, interfaces, and extension methods that handle standard API operations.

### Repository Structure

| Project | Purpose |
|---------|---------|
| `Hamekoz.Api` | Core library — generic services, controllers, DTOs, exceptions, and DI extensions; packaged as a NuGet library |
| `Hamekoz.Api.Example` | Example ASP.NET Core web application demonstrating SDK usage |
| `Hamekoz.Api.Tests` | xUnit test project for the core library |
| `Hamekoz.NET.Sdk.AppHost` | .NET Aspire AppHost for local orchestration |
| `Hamekoz.NET.Sdk.ServiceDefaults` | Shared Aspire defaults: OpenTelemetry, health checks, resilience, service discovery |

---

## Architecture & Key Patterns

### Entity Model
All domain entities inherit from `Entity` (abstract base with `int Id`). Entities never expose public setters for `Id`.

### Generic CRUD Service
`CrudService<TEntity, TDbContext>` provides default EF Core implementations for `GetAllAsync`, `CreateAsync`, `ReadByIdAsync`, `UpdateAsync`, and `DeleteAsync`. All methods accept a `CancellationToken`. Override any method in a derived service for custom logic.

### Generic CRUD Controller
`CrudController<TEntity, TListItemDto, TCreateDto, TDetailDto, TUpdateDto>` exposes five standard REST endpoints. A simpler single-type overload (`CrudController<TEntity>`) is available when one DTO satisfies all operations. Controllers use constructor injection of `IMapper` (AutoMapper) and `ICrudService<TEntity>`.

### DTO Marker Interfaces
All DTOs implement one or more marker interfaces:
- `IListItemDto` — used in GET (list) responses
- `IDetailDto` — used in GET by ID responses
- `ICreateDto` — used in POST request bodies
- `IUpdateDto` — used in PUT request bodies
- `IFullDto` — composite; implements all four (for entities sharing a single DTO)

### Exception Handling
Throw `NotFoundException` (→ HTTP 404) or `ValidationException` (→ HTTP 400) for expected error conditions. `ExceptionHandlingMiddleware` catches these and serializes them to JSON. Register it via `app.UseHamekozMiddlewares()`.

### Dependency Injection
- `services.AddHamekozApi<TDbContext>()` — auto-registers `ICrudService<T>` for every `Entity` subclass found via reflection.
- `services.AddUniqueImplementationOfServices()` — registers interfaces with exactly one implementation.
- `services.AddTemplateServices(...)` — registers generic service templates for all derived types.

### AutoMapper
Entity↔DTO mappings are declared in a `Profile` class (e.g., `MapperProfile`). Use `mapper.Map<TDto>(entity)` for reads and `mapper.Map(dto, entity)` for updates (maps onto an existing tracked instance).

---

## Coding Conventions

Follow the rules enforced by `.editorconfig`:

- **Namespaces**: file-scoped (`namespace Foo.Bar;`)
- **Indentation**: 4 spaces (no tabs)
- **Naming**:
  - Types, methods, properties, events: `PascalCase`
  - Interfaces: `IPascalCase`
  - Type parameters: `TPascalCase`
  - Local variables and parameters: `camelCase`
  - Private instance fields: `_camelCase`
  - Private static fields: `s_camelCase`
  - Constants: `PascalCase`
- **Accessibility modifiers**: always explicit
- **`using` directives**: System namespaces first, then a blank line before other namespaces
- **Language features**: prefer null propagation (`?.`), null coalescing (`??`), pattern matching, expression-bodied members for single-expression methods/properties, object/collection initializers
- **Braces**: always use braces for control flow; opening brace on a new line
- **Nullable**: enabled — annotate nullable returns and parameters with `?`
- **Implicit usings**: enabled; no need to add `using System;` etc.

### Commit Messages
Follow the **Conventional Commits** specification (enforced by CI). Examples:
- `feat: add pagination support to CrudService`
- `fix: return 404 when entity is not found`
- `chore: update dependencies`
- `docs: update README`

---

## Build & Test

```bash
# Restore dependencies
dotnet restore

# Build the entire solution
dotnet build

# Run all tests
dotnet test

# Run tests with code coverage
dotnet test --collect:"XPlat Code Coverage"

# Pack the NuGet library
dotnet pack Hamekoz.Api/Hamekoz.Api.csproj
```

**Target framework**: `net9.0`  
**SDK version**: `9.0.311` (see `global.json`; rollforward is `major`)

---

## CI/CD Pipeline

The workflow (`.github/workflows/hamekoz.yml`) runs on every PR to `main` and on pushes to `main` or version tags (`v*.*.*`):

1. **Conventional Commits** — validates commit message format
2. **Continuous Integration** — `dotnet build` + `dotnet test`
3. **Continuous Delivery** — publishes NuGet package to GitHub Packages on version tag pushes

Always ensure `dotnet build` and `dotnet test` pass before opening a PR.

---

## Adding a New Entity (Typical Workflow)

1. Create a model class inheriting `Entity` in the domain project.
2. Add a `DbSet<TEntity>` to the `DbContext`.
3. Create DTOs implementing the appropriate marker interfaces.
4. Add AutoMapper mappings to the `Profile`.
5. Create a controller inheriting `CrudController<TEntity, ...>`.
6. Register services with `AddHamekozApi<TDbContext>()` (automatic via reflection) or call `AddCrudServices<TDbContext>()` explicitly.
7. Register middleware with `app.UseHamekozMiddlewares()`.

For custom service logic, subclass `CrudService<TEntity, TDbContext>` and override the relevant methods.
