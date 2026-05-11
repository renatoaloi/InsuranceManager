# Project Retrospective

*A living document updated after each milestone. Lessons feed forward into future planning.*

## Milestone: v1.1 — Bugfixes & Stability

**Shipped:** 2026-05-11
**Phases:** 1 | **Plans:** 3 | **Sessions:** ~5

### What Was Built
- Fixed Dockerfile.huey CMD format (direct script path)
- Resolved volume configuration conflict (bind mount only)
- Enabled Python unbuffered output for Docker log visibility

### What Worked
- Small targeted fixes effective — 3 plans, 3 commits
- Problem diagnosis via UAT followed by direct fix
- UAT workflow catching issues before shipping

### What Was Inefficient
- UAT gap discovered after Phase 3 complete — should have tested Huey integration earlier
- Python stdout buffering issue took time to diagnose

### Patterns Established
- Always test Docker containers with docker-compose before marking infrastructure complete
- Python unbuffered output (-u flag) needed for daemon mode logging

### Key Lessons
1. Test the full stack (Docker + Huey) in UAT before marking infrastructure complete
2. Python stdout buffering in Docker requires `-u` flag for visibility

### Cost Observations
- Model mix: compact fixes, minimal research needed
- Sessions: ~5 for 3 plans
- Notable: Fast turnaround once root cause identified

---

## Milestone: v1.0 — MVP

**Shipped:** 2026-05-11
**Phases:** 3 | **Plans:** 16 | **Sessions:** ~25

### What Was Built
- Foundation: .NET 10 hexagonal architecture with Proposal/Policy domain entities
- Status Lifecycle + Auth: Huey async queue for proposal status transitions, API Key middleware
- Infrastructure: Docker containerization for API and Huey worker
- CQRS pattern with isolated read/write adapters
- SQLite persistence with EF Core
- Filesystem broker for Huey (Windows + Docker compatible)

### What Worked
- Hexagonal architecture cleanly separated concerns
- CQRS pattern with IProposalReadAdapter worked well
- Filesystem broker for Huey avoided Redis dependency

### What Was Inefficient
- Phase planning underestimated container testing complexity
- Initial UAT gaps in Phase 3 required fixes

### Patterns Established
- Minimal APIs in .NET 10 for clean endpoint definition
- CQRS with dedicated read/write adapters
- API Key middleware for simple authentication

### Key Lessons
1. Hexagonal architecture provides clean separation for testing and evolution
2. Filesystem broker avoids external dependencies while maintaining async queue semantics
3. UAT should test actual deployment, not just unit tests

---

## Cross-Milestone Trends

### Process Evolution

| Milestone | Sessions | Phases | Key Change |
|-----------|----------|--------|------------|
| v1.0 | ~25 | 3 | Foundation + CQRS + Docker |
| v1.1 | ~5 | 1 | Targeted bug fixes, UAT-driven |

### Cumulative Quality

| Milestone | Tests | Coverage | Zero-Dep Additions |
|-----------|-------|----------|-------------------|
| v1.0 | 16 plans, UAT | - | Huey filesystem broker, CQRS read models |
| v1.1 | 3 plans, UAT | - | Python unbuffered output, bind mount fix |

### Top Lessons (Verified Across Milestones)

1. Test full stack (Docker + external services) in UAT before marking phase complete
2. Small targeted phases more efficient than large scope creep
3. Hexagonal + CQRS provides clean testability and evolution path