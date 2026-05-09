# Requirements: Insurance Manager

**Defined:** 2026-05-09
**Core Value:** Proposta de seguro segue fluxo de estados via mensageria assíncrona, resultando em apólice quando aprovada.

## v1 Requirements

Requirements for initial release. Each maps to roadmap phases.

### Proposals

- [ ] **PROP-01**: User can create a proposal with basic fields (client name, coverage type)
- [ ] **PROP-02**: User can list all proposals with optional filters (status, date range)
- [ ] **PROP-03**: User can get a single proposal by ID
- [ ] **PROP-04**: User can change proposal status via async queue (Huey)
- [ ] **PROP-05**: Proposal status transitions: Em Analise → Aprovada or Recusada

### Policies

- [ ] **POLI-01**: Policy is automatically created when approved proposal is contracted
- [ ] **POLI-02**: Policy stores the 32-char insured asset token
- [ ] **POLI-03**: User can list all policies with optional filters
- [ ] **POLI-04**: User can get a single policy by ID

### CQRS

- [ ] **CQRS-01**: Write operations use dedicated command adapters
- [ ] **CQRS-02**: Read operations use dedicated read adapters/projections
- [ ] **CQRS-03**: Read projections optimized for queries (filtered listings)

### Authentication

- [ ] **AUTH-01**: API Key authentication on all endpoints
- [ ] **AUTH-02**: Invalid API Key returns 401 Unauthorized

### Persistence

- [ ] **PERS-01**: SQLite as primary database
- [ ] **PERS-02**: Database adapter follows port/interface pattern (swappable)

### Infrastructure

- [ ] **INFR-01**: Docker container for API
- [ ] **INFR-02**: Huey worker runs as separate process/container
- [ ] **INFR-03**: Huey filesystem broker works on Windows and Docker

## v2 Requirements

Deferred to future release. Tracked but not in current roadmap.

### Notifications

- **NOTF-01**: Webhook events on policy.bound
- **NOTF-02**: Webhook events on status changes

### Observability

- **OBS-01**: Health check endpoint (/health)
- **OBS-02**: Readiness probe endpoint (/ready)
- **OBS-03**: Request correlation IDs

### Audit

- **AUDT-01**: State transition audit trail

## Out of Scope

Explicitly excluded. Documented to prevent scope creep.

| Feature | Reason |
|---------|--------|
| JWT Authentication | API Key is sufficient for service-to-service communication |
| Claims Management | Domain complexity, deferred to v2+ |
| Multi-item insured | Single token per policy per constraints |
| Rating/Premium calculation | Not in domain scope |
| PDF policy documents | Simple JSON policy first |
| Multi-carrier support | Single carrier for v1 |
| Multi-tenant isolation | Single-tenant for v1 simplicity |

## Traceability

Which phases cover which requirements. Updated during roadmap creation.

| Requirement | Phase | Status |
|-------------|-------|--------|
| PROP-01 | Phase 1 | Pending |
| PROP-02 | Phase 1 | Pending |
| PROP-03 | Phase 1 | Pending |
| PROP-04 | Phase 2 | Pending |
| PROP-05 | Phase 2 | Pending |
| POLI-01 | Phase 1 | Pending |
| POLI-02 | Phase 1 | Pending |
| POLI-03 | Phase 1 | Pending |
| POLI-04 | Phase 1 | Pending |
| CQRS-01 | Phase 1 | Pending |
| CQRS-02 | Phase 2 | Pending |
| CQRS-03 | Phase 2 | Pending |
| AUTH-01 | Phase 2 | Pending |
| AUTH-02 | Phase 2 | Pending |
| PERS-01 | Phase 1 | Pending |
| PERS-02 | Phase 1 | Pending |
| INFR-01 | Phase 3 | Pending |
| INFR-02 | Phase 3 | Pending |
| INFR-03 | Phase 3 | Pending |

**Coverage:**
- v1 requirements: 19 total
- Mapped to phases: 19
- Unmapped: 0

---
*Requirements defined: 2026-05-09*
*Last updated: 2026-05-09 after roadmap creation*