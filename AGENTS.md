# Insurance Manager — Agent Guide

## GSD Workflow

Run `/gsd-plan-phase 1` to start Phase 1 execution.

## Stack
- .NET 10
- SQLite
- Huey (mensageria/background tasks)
- CQRS (isolamento de adaptadores)
- Hexagonal / Ports & Adapters / DDD
- Docker

## Domain
- **Proposal**: estados (Em analise, Aprovada, Recusada) → cria Policy
- **Policy**: criada apenas de Proposals aprovadas; registra ativo digital (32-char ID)
- **ProposalService**: criar, listar, alterar status
- **PolicyService**: contratar proposta aprovada

## Conventions
- CQRS: adaptadores isolados para leitura e escrita
- Item segurado = token de ativo digital (32-char ID) na entidade Policy
- Nao contempla itens segurados separadamente

## Phase 1 — Foundation
See `.planning/ROADMAP.md` for success criteria and plans.

## Commands
- `dotnet build` — Build project
- `dotnet test` — Run tests
- `dotnet run --project src/InsuranceManager.Api` — Run API