# Requirements: Insurance Manager

**Defined:** 2026-05-11
**Core Value:** Proposta de seguro segue fluxo de estados via mensageria assíncrona, resultando em apólice quando aprovada.

## v1.2 Requirements

Requirements for hexagonal architecture enforcement.

### Architecture

- [x] **ARCH-01**: Move HueyTaskRunner from InsuranceManager.Application to InsuranceManager.Infrastructure (Phase 5)
- [x] **ARCH-02**: Create IQueueTaskAdapter port interface in Domain layer
- [x] **ARCH-03**: Implement QueueTaskRunnerAdapter in Infrastructure that implements IQueueTaskAdapter
- [x] **ARCH-04**: Inject IQueueTaskAdapter via dependency injection in Application services
- [x] **ARCH-05**: Verify Domain layer has no direct dependencies on Infrastructure or Application layers

## v2 Requirements

Deferred to future release. Tracked but not in current roadmap.

### Policy Enhancements

- **POL-01**: Policy renewal endpoint
- **POL-02**: Policy cancellation with reason tracking

### Reporting

- **RPT-01**: Dashboard with proposal statistics
- **RPT-02**: Policy conversion rate metrics

## Out of Scope

| Feature | Reason |
|---------|--------|
| Frontend/UI | API-first, frontend separate |
| JWT Authentication | API Key sufficient for v1 |
| Multiple persistence adapters | SQLite only for now |
| Claims management | Defer to future |

## Traceability

| Requirement | Phase | Status |
|-------------|-------|--------|
| ARCH-01 | Phase 5 | Complete |
| ARCH-02 | Phase 5 | Complete |
| ARCH-03 | Phase 5 | Complete |
| ARCH-04 | Phase 6 | Complete |
| ARCH-05 | Phase 6 | Complete |

**Coverage:**
- v1.2 requirements: 5 total
- Mapped to phases: 5
- Unmapped: 0

---
*Requirements defined: 2026-05-11*
*Last updated: 2026-05-11 after v1.2 milestone started*