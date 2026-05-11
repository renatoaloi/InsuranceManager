# Insurance Manager

## What This Is

Sistema de seguros simples com API REST em .NET 10. Gerencia o ciclo de vida de propostas de seguro (criar, listar, alterar status) e contratação gerando apólice. Exemplo de arquitetura hexagonal com mensageria.

## Core Value

Proposta de seguro segue fluxo de estados via mensageria assíncrona, resultando em apólice quando aprovada.

## Current State (v1.0 Shipped)

**Shipped:** 2026-05-11
**Version:** v1.0 MVP
**Stack:** .NET 10, C# 12, SQLite, Huey (filesystem broker), Docker
**Architecture:** Hexagonal / Ports & Adapters / DDD
**Pattern:** CQRS com adaptadores isolados para leitura e escrita

**What Works:**
- Proposal CRUD (create, list, get by ID)
- Proposal status transitions via Huey queue (Em Analise → Aprovada/Recusada)
- Policy auto-creation when contracting approved proposal
- API Key authentication (X-API-Key header)
- SQLite persistence with EF Core
- Docker containerization (API + Huey worker)

**Known Issues (v1.1 scope):**
- Huey container startup needs verification
- Worker connectivity to filesystem broker needs testing

## Requirements

### Validated

- [x] Proposta: criar, listar, alterar status via fila (Huey) — v1.0
- [x] Proposta: transição de estado "Em Análise" → "Aprovada" ou "Recusada" — v1.0
- [x] Apólice: criada automaticamente ao contratar proposta aprovada — v1.0
- [x] Consulta: listar propostas e apólices — v1.0
- [x] Projeções de leitura CQRS (read models) — v1.0
- [x] Autenticação via API Key — v1.0
- [x] Persistência SQLite (adaptador agnóstico por design) — v1.0
- [x] Huey com broker filesystem (funciona em Windows e Docker) — v1.0

### Active

- [ ] Huey container inicia e conecta ao broker
- [ ] Huey worker processa tasks da fila filesystem
- [ ] Verificação end-to-end do processamento de tasks

### Out of Scope

- Autenticação JWT (API Key apenas)
- Múltiplos itens segurados (token único de ativo digital por apólice)
- Sinistros
- Múltiplos adapters de persistência (só SQLite v1)
- Autenticação multi-usuário
- Background workers externos (Huey integrado)

## Context

- Stack: .NET 10, C#, SQLite, Huey, Docker
- Arquitetura: Hexagonal / Ports & Adapters / DDD
- Pattern: CQRS com adaptadores isolados para leitura e escrita
- Huey broker: filesystem (Windows + Docker compat)
- Domínio: Proposal (estados: Em Análise, Aprovada, Recusada) → Policy (criada de Proposal aprovada)

## Constraints

- **Stack**: .NET 10, C# 12 — não sair do ecossistema .NET
- **Arquitetura**: Hexagonal + CQRS — isolar adaptadores de leitura/escrita
- **Persistência**: Banco agnóstico — adapter SQLite apenas por enquanto
- **Mensageria**: Huey — alteração de status via fila distribuída
- **Autenticação**: API Key — simples, para frontend/serviços
- **Huey broker**: Filesystem — compatibilidade Windows + Docker
- **Item segurado**: Token 32-char na Policy — não criar entidade separada

## Key Decisions

| Decision | Rationale | Outcome |
|----------|-----------|---------|
| API Key para auth | Simplicidade — frontend e serviços | ✅ Works in v1.0 |
| Huey filesystem broker | Compatibilidade Windows + Docker, sem dependência Redis | ✅ Configured, testing in v1.1 |
| Item segurado = token 32-char na Policy | Reduzir complexidade v1 | ✅ Implemented |
| Projeções CQRS via read models | Consultas separadas de escrita | ✅ IProposalReadAdapter implemented |

## Next Milestone Goals (v1.1)

**Goal:** Fix Huey container startup issues and any critical bugs from v1.0

**Target features:**
- Huey container starts and connects to broker
- Huey worker processes tasks from filesystem queue
- End-to-end verification of task processing

---

*Last updated: 2026-05-11 after v1.0 milestone shipped*
*Archived: .planning/milestones/v1.0-ROADMAP.md, v1.0-REQUIREMENTS.md*