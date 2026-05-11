# Milestones

## v1.1 Bugfixes & Stability

**Shipped:** 2026-05-11
**Phases:** 1 (Phase 4)
**Plans:** 3
**Tasks:** 3

### Key Accomplishments

1. Fixed Dockerfile.huey CMD format (direct script approach)
2. Resolved volume configuration conflict (removed named volume, bind mount works)
3. Enabled Python unbuffered output for proper log visibility

### Known Gaps

None

---

## v1.0 MVP

**Shipped:** 2026-05-11
**Phases:** 3 (Phases 1-3)
**Plans:** 16
**Tasks:** 16

### Key Accomplishments

1. Foundation: .NET 10 hexagonal architecture with Proposal/Policy domain entities
2. Status Lifecycle + Auth: Huey async queue for proposal status transitions, API Key middleware
3. Infrastructure: Docker containerization for API and Huey worker
4. CQRS pattern with isolated read/write adapters
5. SQLite persistence with EF Core
6. Filesystem broker for Huey (Windows + Docker compatible)

### Known Gaps

- Huey container startup issues (fixed in v1.1)
- Worker connectivity testing (verified in v1.1)

---

*Last updated: 2026-05-11 after v1.1 milestone shipped*