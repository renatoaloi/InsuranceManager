# Roadmap: Insurance Manager

## Milestones

- ✅ **v1.0 MVP** — Phases 1-3 (shipped 2026-05-11)
- ✅ **v1.1 Bugfixes & Stability** — Phase 4 (shipped 2026-05-11)
- ✅ **v1.2 Hexagonal Architecture Enforcement** — Phases 5-6 (shipped 2026-05-11)

## Phases

<details>
<summary>✅ v1.0 MVP (Phases 1-3) — SHIPPED 2026-05-11</summary>

- [x] Phase 1: Foundation (6/6 plans) — completed 2026-05-09
- [x] Phase 2: Status Lifecycle + Auth (6/6 plans) — completed 2026-05-09
- [x] Phase 3: Infrastructure (4/4 plans) — completed 2026-05-10

</details>

<details>
<summary>✅ v1.1 Bugfixes & Stability (Phase 4) — SHIPPED 2026-05-11</summary>

- [x] Phase 4: Huey Bugfixes (3/3 plans) — completed 2026-05-11

</details>

### ✅ v1.2 Hexagonal Architecture Enforcement (Complete)

**Milestone Goal:** Fix core leaks by moving HueyTaskRunner from Application layer to Infrastructure, connected via port/adapter pattern.

- [x] **Phase 5: Port & Adapter Foundation** - Create IQueueTaskAdapter port interface and QueueTaskRunnerAdapter (completed 2026-05-11)
- [x] **Phase 6: Integration & Architecture Validation** - Wire up DI and verify Domain layer isolation (completed 2026-05-11)

## Phase Details

### Phase 5: Port & Adapter Foundation
**Goal**: Domain/Application layers define the contract for task execution; Infrastructure provides the implementation
**Depends on**: Phase 4
**Requirements**: ARCH-01, ARCH-02, ARCH-03
**Success Criteria** (what must be TRUE):
  1. HueyTaskRunner class no longer exists in InsuranceManager.Application
  2. IQueueTaskAdapter interface exists in InsuranceManager.Domain (port layer)
  3. QueueTaskRunnerAdapter class exists in InsuranceManager.Infrastructure implementing IQueueTaskAdapter
  4. Application services that execute tasks depend on IQueueTaskAdapter abstraction, not concrete implementation
  5. Solution builds successfully with QueueTaskRunnerAdapter in Infrastructure
**Plans**: 1 plan
Plans:
- [x] 05-01-PLAN.md — Create IQueueTaskAdapter port in Domain and QueueTaskRunnerAdapter in Infrastructure

### Phase 6: Integration & Architecture Validation
**Goal**: Application services use injected task runner; Domain layer verified to have zero external dependencies
**Depends on**: Phase 5
**Requirements**: ARCH-04, ARCH-05
**Status**: Complete (2026-05-11)
**Success Criteria** (what must be TRUE):
  1. IQueueTaskAdapter is registered in DI container and injected into consuming Application services
  2. dotnet build --list-deps (or equivalent) shows Domain project has no references to Infrastructure or Application
  3. Domain layer project file contains only its own assembly and any domain primitives (no external packages from outer layers)
  4. No using statements in Domain layer files reference InsuranceManager.Infrastructure or InsuranceManager.Application namespaces
  5. All task queue operations flow through IQueueTaskAdapter port without Domain knowing about Huey specifics
**Plans**: 1 plan
Plans:
- [x] 06-01-PLAN.md — Verify architecture tests pass and align documentation naming

## Progress

**Execution Order:**
Phases execute in numeric order: 5 → 6

| Phase | Milestone | Plans Complete | Status | Completed |
|-------|-----------|----------------|--------|-----------|
| 5. Port & Adapter Foundation | v1.2 | 1/1 | Complete | 2026-05-11 |
| 6. Integration & Architecture Validation | v1.2 | 1/1 | Complete | 2026-05-11 |

---

*Roadmap created: 2026-05-11*
*Last updated: 2026-05-11 for v1.2 milestone*
*Previous: .planning/milestones/v1.1-ROADMAP.md*