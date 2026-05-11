# Insurance Manager — Agent Guide

## Stack
- .NET 10 / SQLite / Huey (Python) / Docker
- Hexagonal architecture (Ports & Adapters)
- CQRS with isolated read/write adapters

## Domain
- **Proposal**: lifecycle states (Em analise → Aprovada/Recusada)
- **Policy**: created only from approved proposals; stores 32-char asset token
- Services: `ProposalService` (create, list, status changes), `PolicyService` (contract approved proposals)

## Architecture
```
src/
├── InsuranceManager.Api/        # Controllers, DTOs, middleware
├── InsuranceManager.Application/ # Commands, services, Huey task runner
├── InsuranceManager.Domain/     # Entities, value objects, port interfaces
└── InsuranceManager.Infrastructure/ # Adapters (repositories, read adapters, EF Core)
```

## Commands
- `dotnet build` — Build all projects
- `dotnet test` — Run tests
- `dotnet run --project src/InsuranceManager.Api` — Run API
- `docker-compose up --build` — Run API + Huey worker

## API Access
All endpoints require `X-API-Key` header. Set via `API_KEY` env var (docker-compose uses `${API_KEY}`).

## Huey Background Tasks
- Queue uses filesystem broker (`./huey_data` directory)
- Worker runs as separate container/process via `Dockerfile.huey`
- On Windows, requires Python with `huey` package installed

## Phase Status
- Phase 1 (Foundation): Complete ✅
- Phase 2 (Status Lifecycle + Auth): Complete ✅
- Phase 3 (Infrastructure): Complete ✅

**v1.0 milestone achieved**

See `.planning/ROADMAP.md` for detailed success criteria.