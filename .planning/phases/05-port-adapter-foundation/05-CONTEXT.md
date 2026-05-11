# Phase 5: Port & Adapter Foundation - Context

**Gathered:** 2026-05-11
**Status:** Ready for planning

<domain>
## Phase Boundary

Move HueyTaskRunner from InsuranceManager.Application to Infrastructure layer, creating the IHueyTaskRunner port interface and HueyTaskRunnerAdapter. This enforces hexagonal architecture by separating core domain/application logic from infrastructure concerns.

</domain>

<decisions>
## Implementation Decisions

### Interface Location
- **D-01:** IHueyTaskRunner interface lives in InsuranceManager.Domain layer. The domain defines core abstractions — this is the cleanest hexagonal approach where Domain owns the port contract.

### Adapter Configuration
- **D-02:** HueyTaskRunnerAdapter receives configuration via IConfiguration injection in its constructor. This follows the standard DI pattern already established in the codebase (ProposalService already uses constructor injection).

### Adapter Namespace
- **D-03:** HueyTaskRunnerAdapter is placed in InsuranceManager.Infrastructure.Huey namespace, creating a dedicated Huey folder. This matches how Application.Huey currently organizes Huey-related code and keeps infrastructure adapters grouped by integration type.

</decisions>

<canonical_refs>
## Canonical References

**Downstream agents MUST read these before planning or implementing.**

### Architecture
- `.planning/ROADMAP.md` — Phase 5 goal, success criteria, dependency on Phase 4
- `.planning/REQUIREMENTS.md` — ARCH-01, ARCH-02, ARCH-03 requirements for Phase 5
- `.planning/PROJECT.md` — v1.2 milestone context, hexagonal architecture goals

### Implementation References
- `src/InsuranceManager.Application/Huey/HueyTaskRunner.cs` — Current HueyTaskRunner with IHueyTaskRunner interface (to be moved)
- `src/InsuranceManager.Application/Services/ProposalService.cs` — Consumer of IHueyTaskRunner (line 13-15, 52-58)
- `src/InsuranceManager.Infrastructure/InsuranceManager.Infrastructure.csproj` — Infrastructure project references Domain
- `src/InsuranceManager.Domain/InsuranceManager.Domain.csproj` — Domain project has no external dependencies (clean)
- `src/InsuranceManager.Api/Program.cs` — Current DI registration (line 25)

</canonical_refs>

<codebase_context>
## Existing Code Insights

### Reusable Assets
- IHueyTaskRunner interface: Already defined and in use. The interface signature (EnqueueStatusChangeAsync) is the contract that must be preserved.
- ProposalService: Already depends on IHueyTaskRunner via constructor injection — minimal refactoring needed.

### Established Patterns
- Constructor injection: Services receive dependencies via constructor (IConfiguration, repositories)
- Namespace per integration: Huey code lives in Application.Huey namespace — adapter will mirror this in Infrastructure.Huey
- Port/Adapter separation: Infrastructure project references Domain only — clean dependency direction

### Integration Points
- DI registration in Program.cs: Currently registers IHueyTaskRunner → HueyTaskRunner
- ProposalService: Calls _hueyTaskRunner.EnqueueStatusChangeAsync() when status changes

</codebase_context>

<specifics>
## Specific Ideas

No specific requirements — open to standard approaches following existing hexagonal patterns.

</specifics>

<deferred>
## Deferred Ideas

None — discussion stayed within phase scope

</deferred>

---

*Phase: 5-Port & Adapter Foundation*
*Context gathered: 2026-05-11*
