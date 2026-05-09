# Project Research Summary

**Project:** Insurance Manager API
**Domain:** Insurance Management (P&C Simplified)
**Researched:** 2026-05-09
**Confidence:** MEDIUM-HIGH

## Executive Summary

This is a simplified insurance management API focused on the core proposal-to-policy workflow. The domain is well-understood with clear patterns from established insurance platforms (BindHQ, BriteCore, Boost). Research confirms .NET 10 with Minimal APIs is the recommended approach — Microsoft explicitly promotes Minimal APIs for new projects as they have lower overhead and better performance than MVC.

The key differentiation from competitors is the async status processing via Huey message queue (unique among competitors who use synchronous APIs) and explicit CQRS architecture with hexagonal patterns. The scope is intentionally narrow: single token per policy, no claims management, no rating engine. This simplicity is a strategic advantage — competitors offer full-suite platforms while we win on focused, maintainable code.

**Primary risk:** The async status processing via Huey is a differentiating pattern but requires careful integration between .NET and Python queues. Mitigation: Use filesystem broker for Windows+Docker compatibility, implement health checks for queue monitoring.

## Key Findings

### Recommended Stack

**Core technologies:**
- .NET 10 SDK — Latest LTS with native AOT support, Microsoft's standard for new APIs
- ASP.NET Core 10.0 with Minimal APIs — Less boilerplate than MVC, recommended by Microsoft
- Entity Framework Core 10.0.2 — Official Microsoft ORM, first-class SQLite support, LTS until November 2028
- SQLite 3.x — Lightweight, file-based, zero-config, works with filesystem broker for Huey

**Supporting libraries:**
- Carter 8.x — Minimal API organization, modular route handling
- FluentValidation 11.9.2 — Complex validation beyond DataAnnotations
- Serilog 4.0 — Structured logging with multiple sinks
- AspNetCore.SecurityKey 4.x — API key authentication (per project constraints)
- Huey 3.x — Python message queue with filesystem broker

### Expected Features

**Must have (table stakes):**
- Proposal CRUD API — Core domain interaction (POST/GET/PATCH)
- Status Lifecycle — Transitions "Em Análise" → "Aprovada" or "Recusada" via async queue
- Policy Creation — Automatic when approved proposal is "contratado"
- Policy Retrieval — Get by ID with insured token (32-char)
- API Key Authentication — Per constraints
- Proposal Listing — Filter by status, date range

**Should have (differentiators):**
- Async Status Updates via Huey — Unique vs competitors, enables retry, prevents blocking
- CQRS Read/Write Separation — Query optimization, isolated adapters
- Hexagonal Architecture — Swappable persistence (SQLite → PostgreSQL later)
- Webhook Notifications — Event-driven integrations (policy.bound events)
- Audit Trail — State transition history for compliance

**Defer (v2+):**
- Claims Management — Domain onto itself, out of scope per constraints
- Multi-Item Insured — Single token per policy per constraints
- Rating/Premium Calculation — Actuarial complexity
- PDF Policy Documents — Templating and legal language
- Multi-Carrier Support — Carrier-specific rules
- JWT Authentication — Only if multi-user scenario added

### Architecture Approach

**NOTE:** ARCHITECTURE.md was not created by researchers. Architecture will be defined during implementation planning based on stack choices.

**Expected major components:**
1. Proposal Domain — Entity with state machine, CRUD operations
2. Policy Domain — Created from approved proposals, holds insured token
3. Huey Integration Service — Async status processing, queue monitoring
4. CQRS Adapters — Read (queries) and Write (commands) separated

### Critical Pitfalls

**NOTE:** PITFALLS.md was not created by researchers. Risks will be identified during implementation.

Based on available research, potential pitfalls:
1. **Huey-.NET Integration Complexity** — Python queue + .NET consumer requires careful file parsing or HTTP API integration
2. **State Machine Validation** — Ensure invalid transitions are rejected (Em Análise → only Aprovada/Recusada)
3. **CQRS Consistency** — Read models must stay synchronized with write models

## Implications for Roadmap

Based on research, suggested phase structure:

### Phase 1: Core Domain Setup
**Rationale:** Foundation must come first — domain entities, persistence, basic CRUD
**Delivers:** Proposal and Policy entities, EF Core SQLite setup, basic read/write endpoints
**Addresses:** Proposal CRUD, Policy creation, SQLite persistence
**Avoids:** Integration complexity — build domain first, add Huey later

### Phase 2: Status Lifecycle + Huey Integration
**Rationale:** Core differentiation — async status processing via message queue
**Delivers:** State machine, Huey consumer, async status updates
**Addresses:** Status Lifecycle (async via Huey), Audit Trail basics
**Avoids:** Synchronous blocking — all status changes go through queue

### Phase 3: CQRS Read Models + API Refinement
**Rationale:** Query optimization, separated read adapters
**Delivers:** Read adapters, proposal listing with filters, policy retrieval
**Addresses:** Proposal Listing with Filters, CQRS Read Models

### Phase 4: Production Readiness
**Rationale:** What you ship with
**Delivers:** API key authentication, Swagger/OpenAPI docs, health endpoints, error handling
**Addresses:** API Key Authentication, REST API Documentation, Health Endpoints

### Phase Ordering Rationale

- Domain first (Phase 1) because all other features depend on Proposal/Policy entities
- Async Huey integration (Phase 2) builds on domain, adds the unique differentiation
- CQRS (Phase 3) optimizes queries after core operations work
- Production readiness (Phase 4) comes last because it's cross-cutting

### Research Flags

Phases likely needing deeper research during planning:
- **Phase 2 (Huey Integration):** Complex Python-.NET integration, needs file monitoring or HTTP API research
- **Phase 3 (CQRS):** Read model projections, eventual consistency patterns

Phases with standard patterns (skip research-phase):
- **Phase 1:** Standard CRUD patterns, well-documented EF Core scenarios
- **Phase 4:** Standard API auth, OpenAPI, health checks — established patterns

## Confidence Assessment

| Area | Confidence | Notes |
|------|------------|-------|
| Stack | HIGH | Verified against Microsoft Learn, official package docs |
| Features | HIGH | Clear domain, competitor analysis available |
| Architecture | LOW | No ARCHITECTURE.md created — will need to define during planning |
| Pitfalls | LOW | No PITFALLS.md created — will need to identify during implementation |

**Overall confidence:** MEDIUM-HIGH

Stack and features are well-researched. Architecture and pitfalls need to be defined during implementation planning.

### Gaps to Address

- **Architecture patterns:** Hexagonal architecture specifics need definition (ports, adapters, domain layer boundaries)
- **Huey integration approach:** File monitoring vs HTTP API — needs decision before Phase 2
- **CQRS implementation:** Read model strategy (projections, materialized views, or simple queries) — needs decision before Phase 3

## Sources

### Primary (HIGH confidence)
- Microsoft Learn: ASP.NET Core Best Practices
- Microsoft Learn: EF Core 10.0 What's New
- Carter GitHub: README and documentation
- FluentValidation: ASP.NET Core integration

### Secondary (MEDIUM confidence)
- BindHQ API documentation (api.bindhq.com) — Policy lifecycle patterns
- BriteCore API documentation (api-docs.britecore.com) — Cloud-native P&C features
- Boost Insurance API Guide (learn.boostinsurance.com) — Full-cycle insurance patterns
- Telerik Blog: Organizing Minimal APIs with Carter

### Tertiary (LOW confidence)
- Various blog posts on API Key authentication — needs validation during implementation

---

*Research completed: 2026-05-09*
*Available research: STACK.md, FEATURES.md*
*Missing: ARCHITECTURE.md, PITFALLS.md*
*Ready for roadmap: yes (with noted gaps)*