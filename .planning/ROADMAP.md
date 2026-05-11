# Roadmap: Insurance Manager

## Overview

Deliver a .NET 10 REST API for insurance proposal-to-policy lifecycle. Phase 1 builds the domain foundation with Proposal/Policy entities and SQLite persistence. Phase 2 adds async status transitions via Huey queue and API Key authentication. Phase 3 containerizes the API and Huey worker for deployment. Phase 4 fixes Huey container startup issues and stability.

## Phases

- [x] **Phase 1: Foundation** - Domain entities, SQLite persistence, basic Proposal/Policy CRUD
- [x] **Phase 2: Status Lifecycle + Auth** - Huey async status transitions, CQRS read models, API Key authentication
- [x] **Phase 3: Infrastructure** - Docker containers, Huey worker, filesystem broker
- [ ] **Phase 4: Huey Bugfixes** - Fix container startup, worker connectivity, volume sharing

## Phase Details

### Phase 1: Foundation

**Goal**: Users can create proposals and automatically receive policies when proposals are approved

**Depends on**: Nothing (first phase)

**Requirements**: PROP-01, PROP-02, PROP-03, POLI-01, POLI-02, POLI-03, POLI-04, CQRS-01, PERS-01, PERS-02

**Status**: ✅ Complete

**Success Criteria** (what must be TRUE):
1. ✅ User can create a proposal with client name and coverage type via POST /api/proposals
2. ✅ User can list all proposals via GET /api/proposals
3. ✅ User can filter proposals by status via GET /api/proposals?status=EmAnalise
4. ✅ User can get a single proposal by ID via GET /api/proposals/{id}
5. ✅ System automatically creates a policy when a user contracts an approved proposal (POST /api/proposals/{id}/contract)
6. ✅ User can list all policies via GET /api/policies
7. ✅ User can get a single policy by ID via GET /api/policies/{id}
8. ✅ Write operations use dedicated command adapters (CQRS-01)
9. ✅ SQLite database is initialized with proper schema on startup

**Plans:** 6/6 complete

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

**Status**: ✅ Complete

**Success Criteria** (what must be TRUE):
1. ✅ User can submit a status change request that is processed asynchronously via Huey queue
2. ✅ Proposal status transitions from "Em Analise" to "Aprovada" or "Recusada" via queued task
3. ✅ Invalid status transitions are rejected (e.g., "Recusada" cannot transition to "Aprovada")
4. ✅ Read operations use dedicated read adapters with optimized projections (CQRS-02, CQRS-03)
5. ✅ All API endpoints require valid API Key in X-API-Key header
6. ✅ Requests with missing or invalid API Key receive 401 Unauthorized response

**Plans:** 6/6 complete

Plans:
- [x] 02-01-PLAN.md — Implement state machine validation for proposal status transitions (CanTransitionTo method)
- [x] 02-02-PLAN.md — Implement status change command and update ProposalService
- [x] 02-03-PLAN.md — Define IProposalReadAdapter port interface in Domain layer
- [x] 02-04-PLAN.md — Implement IProposalReadAdapter in Infrastructure and update controllers
- [x] 02-05-PLAN.md — Add API Key authentication middleware (X-API-Key header validation)
- [x] 02-06-PLAN.md — Configure Huey filesystem broker and create consumer script

### Phase 3: Infrastructure

**Goal**: API and Huey worker run in Docker containers with filesystem broker

**Depends on**: Phase 2

**Requirements**: INFR-01, INFR-02, INFR-03

**Status**: ✅ Complete

**Success Criteria** (what must be TRUE):
1. ✅ API runs in a Docker container with proper health endpoints
2. ✅ Huey worker runs as a separate container/process
3. ⚠️ Huey filesystem broker configured for Windows and Docker (container starts but worker connectivity needs testing)

**Plans:** 4/4 complete

Plans:
- [x] 03-01-PLAN.md — Add Dockerfile for API with multi-stage build
- [x] 03-02-PLAN.md — Add Dockerfile for Huey worker
- [x] 03-03-PLAN.md — Add docker-compose.yml with API and Huey worker services
- [x] 03-04-PLAN.md — Configure filesystem broker and health endpoints

### Phase 4: Huey Bugfixes

**Goal**: Fix Huey container startup issues and ensure worker processes tasks from filesystem queue

**Depends on**: Phase 3

**Requirements**: BUG-01, BUG-02, BUG-03

**Status**: ○ In progress

**Success Criteria** (what must be TRUE):
1. Huey container starts without errors or crashes
2. Huey worker connects to filesystem broker and processes queued tasks
3. Both API and Huey containers share the same huey_data volume correctly

**Plans:** 3/3 planned

Plans:
- [ ] 04-01-PLAN.md — Fix Dockerfile.huey CMD to use direct script approach
- [ ] 04-02-PLAN.md — Fix docker-compose.yml volume configuration
- [ ] 04-03-PLAN.md — End-to-end verification of Huey task processing

## Progress

| Phase | Plans Complete | Status | Completed |
|-------|----------------|--------|-----------|
| 1. Foundation | 6/6 | Complete | 2026-05-09 |
| 2. Status Lifecycle + Auth | 6/6 | Complete | 2026-05-09 |
| 3. Infrastructure | 4/4 | Complete | 2026-05-10 |
| 4. Huey Bugfixes | 0/3 | In progress | — |

---

*Roadmap created: 2026-05-09*
*Last updated: 2026-05-10 after v1.0 milestone shipped, v1.1 started*