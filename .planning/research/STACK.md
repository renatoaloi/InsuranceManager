# Stack Research

**Domain:** Insurance Management API
**Researched:** 2026-05-09
**Confidence:** HIGH

## Recommended Stack

### Core Technologies

| Technology | Version | Purpose | Why Recommended |
|------------|---------|---------|-----------------|
| .NET SDK | 10.0 | Runtime & SDK | Latest LTS release with native AOT support and performance improvements. Microsoft's standard for new APIs. |
| ASP.NET Core | 10.0 | Web framework | Built into .NET 10, minimal overhead. Microsoft recommends Minimal APIs for new projects — less boilerplate, better performance than MVC. |
| Entity Framework Core | 10.0.2 | ORM | Official Microsoft ORM, first-class SQLite support, LINQ queries, migrations. Version 10 is LTS supported until November 2028. |
| SQLite | 3.x | Database | Lightweight, file-based, zero-config, perfect for v1. Works with filesystem broker for Huey. |

### Supporting Libraries

| Library | Version | Purpose | When to Use |
|---------|---------|---------|-------------|
| Carter | 8.x | Minimal API organization | Organizing routes into modules, avoiding bloated Program.cs. Provides FluentValidation integration. |
| FluentValidation | 11.9.2 | Validation | Complex validation rules beyond DataAnnotations. Carter has built-in support. |
| Serilog | 4.0 | Structured logging | Production logging with sinks (console, file). Recommended over default ILogger for structured output. |
| AspNetCore.SecurityKey | 4.x | API Key authentication | Production-ready API key validation middleware. Supports multiple keys, caching, OpenAPI integration. |
| Swashbuckle.AspNetCore | 7.x | OpenAPI/Swagger | API documentation. Native support in .NET 10 minimal APIs. |
| Huey | 3.x | Message queue | Python task queue. Use filesystem broker for Windows + Docker compatibility. |

### Development Tools

| Tool | Purpose | Notes |
|------|---------|-------|
| dotnet CLI | Build & run | `dotnet run` from project root |
| dotnet-ef | Migrations | `dotnet tool install -g dotnet-ef` |
| Docker | Containerization | For Huey consumer and SQLite in production |
| VS Code / Rider | IDE | Both support .NET 10 development |

## Installation

```bash
# Install .NET 10 SDK
# https://dotnet.microsoft.com/download/dotnet/10.0

# Create new minimal API project
dotnet new web -n InsuranceManager -o .

# Add core packages
dotnet add package Microsoft.EntityFrameworkCore.Sqlite --version 10.0.2
dotnet add package Microsoft.EntityFrameworkCore.Design --version 10.0.2
dotnet add package Carter --version 8.1.0
dotnet add package FluentValidation --version 11.9.2
dotnet add package FluentValidation.DependencyInjectionExtensions --version 11.9.2
dotnet add package Serilog.AspNetCore --version 8.0.0
dotnet add package Serilog.Sinks.Console --version 6.0.0
dotnet add package AspNetCore.SecurityKey --version 4.1.0
dotnet add package Swashbuckle.AspNetCore --version 7.2.0

# Install EF tools globally
dotnet tool install -g dotnet-ef
```

## Alternatives Considered

| Recommended | Alternative | When to Use Alternative |
|-------------|-------------|-------------------------|
| Minimal APIs + Carter | MVC Controllers | Only if team has strong MVC background and API will have 40+ endpoints with complex filters. Performance difference is negligible. |
| EF Core 10 | Dapper | Only if extreme performance needed (millisecond-level queries). EF Core 10 has bulk operations and ExecuteUpdate improvements. Loses migrations and type safety. |
| Huey (filesystem) | Redis broker | Only if scaling to multiple servers. Redis requires extra infrastructure. Filesystem works for single-instance and Docker. |
| API Key middleware | JWT | Only if multi-user authentication with roles needed. Project specifies API Key only. |
| FluentValidation | DataAnnotations | Only for simple validation. .NET 10 has built-in validation for Minimal APIs with DataAnnotations. FluentValidation preferred for complex rules. |

## What NOT to Use

| Avoid | Why | Use Instead |
|-------|-----|-------------|
| NestJS | Node.js framework, not .NET | Stick to .NET per project constraints |
| MassTransit | Over-engineered for simple queue integration | Direct Huey integration via HTTP or file |
| NHibernate | Legacy, not actively developed | EF Core — modern, supported, LINQ |
| JWT for this project | Overkill for single API key requirement | API Key as specified in requirements |
| Raw minimal APIs (no Carter) | Program.cs becomes unmanageable at scale | Carter for modular organization |

## Stack Patterns by Variant

**If team is small (1-2 devs) and API is simple (< 15 endpoints):**
- Use raw Minimal APIs without Carter
- Use built-in .NET 10 validation with DataAnnotations
- Simpler setup, faster to ship

**If API grows or team expands:**
- Add Carter for modular route organization
- Add FluentValidation for complex business rules
- Use route groups for logical grouping

**If Huey integration via direct file monitoring:**
- Create a background service that watches Huey queue directory
- Parse Huey task files and execute corresponding .NET handlers
- More complex but allows full integration

**If Huey integration via HTTP API:**
- Expose internal API endpoints that Huey Python code calls
- Simpler but requires Python-side modifications

## Version Compatibility

| Package | Compatible With | Notes |
|---------|-----------------|-------|
| EF Core 10.0.2 | .NET 10.0 | LTS release, supports SQLite |
| Carter 8.1.0 | .NET 8+, EF Core 8+ | Check latest for .NET 10 |
| FluentValidation 11.9.2 | .NET 6+, EF Core 6+ | Full .NET 10 support |
| Serilog 8.0 | .NET 8+ | .NET 10 compatible |
| AspNetCore.SecurityKey 4.1.0 | .NET 8+ | .NET 10 compatible as of Feb 2026 |
| Swashbuckle 7.2 | .NET 8+ | Full minimal API support |

## Sources

- Microsoft Learn: ASP.NET Core Best Practices — HIGH confidence
- Microsoft Learn: EF Core 10.0 What's New — HIGH confidence
- CSharp.com: Minimal API vs MVC Controllers in .NET 10 — HIGH confidence
- Carter GitHub: README and documentation — MEDIUM confidence
- FluentValidation: ASP.NET Core integration — MEDIUM confidence
- Telerik Blog: Organizing Minimal APIs with Carter — MEDIUM confidence
- CSharp Corner: API Key Authentication in ASP.NET Core — MEDIUM confidence

---

*Stack research for: Insurance Manager API*
*Researched: 2026-05-09*