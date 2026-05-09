# Roadmap: Insurance Manager API

## Overview

Deliver a .NET 10 REST API for insurance proposal-to-policy lifecycle. Phase 1 builds the domain foundation with Proposal/Policy entities and SQLite persistence. Phase 2 adds async status transitions via Huey queue and API key authentication. Phase 3 containerizes the API and Huey worker for deployment.

## Phases

- [x] **Phase 1: Foundation** - Domain entities, SQLite persistence, basic Proposal/Policy CRUD
- [ ] **Phase 2: Status Lifecycle + Auth** - Huey async status transitions, CQRS read models, API Key authentication
- [ ] **Phase 3: Infrastructure** - Docker containers, Huey worker, filesystem broker

## Phase Details

### Phase 1: Foundation

**Goal**: Users can create proposals and automatically receive policies when proposals are approved

**Depends on**: Nothing (first phase)

**Requirements**: PROP-01, PROP-02, PROP-03, POLI-01, POLI-02, POLI-03, POLI-04, CQRS-01, PERS-01, PERS-02

**Success Criteria** (what must be TRUE):
1. User can create a proposal with client name and coverage type via POST /api/proposals
2. User can list all proposals via GET /api/proposals
3. User can filter proposals by status via GET /api/proposals?status=EmAnalise
4. User can get a single proposal by ID via GET /api/proposals/{id}
5. System automatically creates a policy when a user contracts an approved proposal (POST /api/proposals/{id}/contract)
6. User can list all policies via GET /api/policies
7. User can get a single policy by ID via GET /api/policies/{id}
8. Write operations use dedicated command adapters (CQRS-01)
9. SQLite database is initialized with proper schema on startup

**Plans:** 6 plans

Plans:
- [x] 01-01-PLAN.md — Set up .NET 10 project structure with hexagonal architecture
- [x] 01-02-PLAN.md — Implement Proposal and Policy domain entities
- [x] 01-03-PLAN.md — Configure EF Core with SQLite persistence adapter
- [x] 01-04-PLAN.md — Implement CQRS command adapters for Proposals and Policies
- [x] 01-05-PLAN.md — Build Proposal endpoints (create, list, get by ID)
- [x] 01-06-PLAN.md — Build Policy endpoints (contract, list, get by ID)

### Phase 2: Status Lifecycle + Auth

**Goal**: Proposal status changes execute asynchronously via Huey queue, and all endpoints require valid API Key

**Depends on**: Phase 1

**Requirements**: PROP-04, PROP-05, CQRS-02, CQRS-03, AUTH-01, AUTH-02

**Success Criteria** (what must be TRUE):
1. User can submit a status change request that is processed asynchronously via Huey queue
2. Proposal status transitions from "Em Analise" to "Aprovada" or "Recusada" via queued task
3. Invalid status transitions are rejected (e.g., "Recusada" cannot transition to "Aprovada")
4. Read operations use dedicated read adapters with optimized projections (CQRS-02, CQRS-03)
5. All API endpoints require valid API Key in X-API-Key header
6. Requests with missing or invalid API Key receive 401 Unauthorized response

**Plans**: TBD

Plans:
- [ ] 02-01: Implement Huey consumer service for status transitions
- [ ] 02-02: Add status change endpoint with async queue submission (POST /api/proposals/{id}/status)
- [ ] 02-03: Implement state machine validation for proposal status transitions
- [ ] 02-04: Implement CQRS read adapters with optimized query projections
- [ ] 02-05: Add API Key authentication middleware
- [x] 02-06: Configure Huey filesystem broker for Windows + Docker compatibility

### Phase 3: Infrastructure

**Goal**: API and Huey worker run in Docker containers with filesystem broker

**Depends on**: Phase 2

**Requirements**: INFR-01, INFR-02, INFR-03

**Success Criteria** (what must be TRUE):
1. API runs in a Docker container with proper health endpoints
2. Huey worker runs as a separate container/process
3. Huey filesystem broker works identically on Windows development machine and in Docker

**Plans**: TBD

Plans:
- [ ] 03-01: Create Dockerfile for API container
- [ ] 03-02: Create Dockerfile/entrypoint for Huey worker
- [ ] 03-03: Create docker-compose.yml with API and Huey worker services
- [ ] 03-04: Verify filesystem broker works in Docker environment

## Progress

| Phase | Plans Complete | Status | Completed |
|-------|----------------|--------|-----------|
| 1. Foundation | 6/6 | Complete | 2026-05-09 |
| 2. Status Lifecycle + Auth | 1/6 | In progress | - |
| 3. Infrastructure | 0/4 | Not started | - |

---

*Roadmap created: 2026-05-09*
*Granularity: coarse*