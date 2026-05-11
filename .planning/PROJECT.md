# Insurance Manager

## What This Is

Sistema de seguros simples com API REST em .NET 10. Gerencia o ciclo de vida de propostas de seguro (criar, listar, alterar status) e contratação gerando apólice. Exemplo de arquitetura hexagonal com mensageria.

## Core Value

Proposta de seguro segue fluxo de estados via mensageria assíncrona, resultando em apólice quando aprovada.

## Current Milestone: v1.2 Hexagonal Architecture Enforcement

**Goal:** Fix core leaks by moving HueyTaskRunner from Application layer to Infrastructure, connected via port/adapter pattern.

**Target features:**
- Move HueyTaskRunner to InsuranceManager.Infrastructure
- Create IHueyTaskRunner port interface in Domain/Application layer
- Implement adapter pattern for task queue integration
- Ensure Domain layer has no dependencies on outer layers

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

- [ ] ARCH-01: Move HueyTaskRunner from Application to Infrastructure layer
- [ ] ARCH-02: Create IHueyTaskRunner port interface in Domain/Application
- [ ] ARCH-03: Implement adapter pattern for task queue connection
- [ ] ARCH-04: Verify Domain layer has no external dependencies

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

## Evolution

This document evolves at phase transitions and milestone boundaries.

**After each phase transition** (via `/gsd-transition`):
1. Requirements invalidated? → Move to Out of Scope with reason
2. Requirements validated? → Move to Validated with phase reference
3. New requirements emerged? → Add to Active
4. Decisions to log? → Add to Key Decisions
5. "What This Is" still accurate? → Update if drifted

**After each milestone** (via `/gsd-complete-milestone`):
1. Full review of all sections
2. Core Value check — still the right priority?
3. Audit Out of Scope — reasons still valid?
4. Update Context with current state

---

*Last updated: 2026-05-11 after v1.2 milestone started*
*Previous: v1.1 (Bugfixes & Stability) — shipped 2026-05-11*