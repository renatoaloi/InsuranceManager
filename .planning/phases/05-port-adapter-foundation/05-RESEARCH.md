# Phase 5: Port & Adapter Foundation - Research

**Researched:** 2026-05-11
**Domain:** Hexagonal Architecture / Port & Adapter Pattern in .NET
**Confidence:** HIGH

## Summary

Phase 5 implements the Port & Adapter pattern for Huey task execution in a hexagonal architecture. The current `HueyTaskRunner` class in Application layer violates clean architecture — it couples application logic to infrastructure concerns. This phase moves the concrete implementation to Infrastructure while defining the contract in Domain as a port interface.

**Primary recommendation:** Follow the established repository pattern: `Domain/Ports/IHueyTaskRunner.cs` → `Infrastructure/Huey/HueyTaskRunnerAdapter.cs`, mirroring how `IProposalRepository` is in Domain/Ports and `ProposalRepository` is in Infrastructure/Adapters.

## User Constraints (from CONTEXT.md)

### Locked Decisions
- **D-01:** IHueyTaskRunner interface lives in InsuranceManager.Domain layer
- **D-02:** HueyTaskRunnerAdapter receives configuration via IConfiguration injection
- **D-03:** HueyTaskRunnerAdapter is placed in InsuranceManager.Infrastructure.Huey namespace

### the agent's Discretion
- Implementation details within the locked decisions (file organization, exact class structure)

### Deferred Ideas
None — discussion stayed within phase scope

## Phase Requirements

| ID | Description | Research Support |
|----|-------------|------------------|
| ARCH-01 | Move HueyTaskRunner from InsuranceManager.Application to InsuranceManager.Infrastructure | Pattern confirmed: files move from Application/Huey to Infrastructure/Huey |
| ARCH-02 | Create IHueyTaskRunner port interface in Domain layer | Pattern confirmed: Interface in Domain/Ports following existing IProposalRepository pattern |
| ARCH-03 | Implement HueyTaskRunnerAdapter in Infrastructure that implements IHueyTaskRunner | Pattern confirmed: Adapter in Infrastructure following existing RepositoryAdapter pattern |

## Architectural Responsibility Map

| Capability | Primary Tier | Secondary Tier | Rationale |
|------------|-------------|----------------|-----------|
| Task execution contract definition | Domain | — | Ports define core abstractions — Domain owns the contract |
| Task enqueueing implementation | Infrastructure | — | Huey integration is infrastructure concern |
| Task triggering from business logic | Application | — | ProposalService calls the port, doesn't know about Huey |

## Standard Stack

### Core
| Library | Version | Purpose | Why Standard |
|---------|---------|---------|--------------|
| Microsoft.Extensions.Configuration.Abstractions | 10.0.0 | IConfiguration injection | Standard DI pattern already in use (ProposalService) |

### Existing Patterns Used
| Pattern | Where | Purpose |
|---------|-------|---------|
| Port interface in Domain | Domain/Ports/IProposalRepository.cs | Defines contract without implementation |
| Adapter in Infrastructure | Infrastructure/Adapters/ProposalRepository.cs | Implements port for persistence |
| Constructor injection | ProposalService.cs | Receives dependencies via constructor |
| DI registration | Program.cs | Wires interface → implementation |

## Architecture Patterns

### System Architecture Diagram

```
┌─────────────────────────────────────────────────────────────────┐
│                      InsuranceManager.Api                       │
│                         (Composition Root)                       │
│                                                                 │
│  builder.Services.AddScoped<IHueyTaskRunner, HueyTaskRunnerAdapter>()│
└────────────────────────────┬────────────────────────────────────┘
                             │ injects
                             ▼
┌─────────────────────────────────────────────────────────────────┐
│                 InsuranceManager.Application                     │
│                                                                 │
│  ProposalService ───────────────────────────────────────────────│
│  │                                                               │
│  └──(uses)─→ IHueyTaskRunner  ←──── (port interface)           │
│                    │                                           │
└────────────────────┼───────────────────────────────────────────┘
                     │ implements
                     ▼
┌─────────────────────────────────────────────────────────────────┐
│               InsuranceManager.Infrastructure                    │
│                                                                 │
│  HueyTaskRunnerAdapter ─────────────────────────────────────────│
│  │                                                               │
│  └──(uses)─→ IConfiguration                                    │
│                    │                                           │
│                    └──(reads)──→ Huey:QueuePath                  │
└─────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────┐
│                 InsuranceManager.Domain                          │
│                                                                 │
│  Ports/IHueyTaskRunner.cs ──────────────────────────────────────│
│  │ (interface only — no implementation)                        │
└─────────────────────────────────────────────────────────────────┘
```

### Recommended Project Structure
```
src/
├── InsuranceManager.Domain/
│   └── Ports/
│       ├── IProposalRepository.cs   (existing)
│       ├── IPolicyRepository.cs      (existing)
│       └── IHueyTaskRunner.cs        (NEW - Phase 5)
├── InsuranceManager.Application/
│   ├── Huey/
│   │   └── (HueyTaskRunner.cs — REMOVED in Phase 5)
│   └── Services/
│       └── ProposalService.cs       (uses IHueyTaskRunner)
├── InsuranceManager.Infrastructure/
│   ├── Huey/
│   │   └── HueyTaskRunnerAdapter.cs (NEW - Phase 5)
│   ├── Adapters/
│   │   ├── ProposalRepository.cs    (existing pattern)
│   │   └── PolicyRepository.cs      (existing pattern)
│   └── Persistence/
│       └── InsuranceDbContext.cs
└── InsuranceManager.Api/
    └── Program.cs                   (DI registration updated)
```

### Pattern: Port & Adapter Migration

**What:** Move a concrete infrastructure implementation from Application to Infrastructure, define its contract as a port in Domain.

**When to use:** When Application layer contains infrastructure code (file I/O, external APIs, queues).

**Steps:**
1. Define port interface in Domain/Ports (e.g., `IHueyTaskRunner.cs`)
2. Move implementation to Infrastructure/Huey (e.g., `HueyTaskRunnerAdapter.cs`)
3. Rename class to `<OldName>Adapter` to clarify role
4. Update DI registration in Program.cs
5. Update using statements in consuming services
6. Delete old implementation file from Application

**Example (Domain port):**
```csharp
// src/InsuranceManager.Domain/Ports/IHueyTaskRunner.cs
using InsuranceManager.Domain.ValueObjects;

namespace InsuranceManager.Domain.Ports;

public interface IHueyTaskRunner
{
    Task EnqueueStatusChangeAsync(Guid proposalId, ProposalStatus newStatus, CancellationToken ct = default);
}
```

**Example (Infrastructure adapter):**
```csharp
// src/InsuranceManager.Infrastructure/Huey/HueyTaskRunnerAdapter.cs
using Microsoft.Extensions.Configuration;
using InsuranceManager.Domain.Ports;
using InsuranceManager.Domain.ValueObjects;

namespace InsuranceManager.Infrastructure.Huey;

public class HueyTaskRunnerAdapter : IHueyTaskRunner
{
    private readonly string _hueyDir;

    public HueyTaskRunnerAdapter(IConfiguration configuration)
    {
        _hueyDir = configuration["Huey:QueuePath"] ?? "/app/huey_data";
    }

    public Task EnqueueStatusChangeAsync(Guid proposalId, ProposalStatus newStatus, CancellationToken ct = default)
    {
        // Same implementation as current HueyTaskRunner
        // ... existing code ...
    }
}
```

### Anti-Patterns to Avoid

- **Don't add Huey packages to Domain:** Domain must remain clean with zero external dependencies. Configuration comes from adapter, not domain.
- **Don't create new infrastructure folder for single adapter:** Placing HueyTaskRunnerAdapter in `Infrastructure/Huey/` follows D-03 decision. This is the correct location per user decision.
- **Don't keep IHueyTaskRunner in Application:** Per D-01, the interface goes in Domain. This is the correct decision per hexagonal architecture.

## Don't Hand-Roll

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|-------------|-----|
| Task execution abstraction | Custom delegate or static class | Interface + DI | Follows hexagonal pattern, testable |
| Configuration access in Domain | Static config access or environment variables | IConfiguration in adapter | Keeps Domain clean, infrastructure handles config |

**Key insight:** The current HueyTaskRunner is already hand-rolled infrastructure code in Application. Moving it follows the pattern already established with repositories.

## Common Pitfalls

### Pitfall 1: Circular Dependencies
**What goes wrong:** Infrastructure references Domain, Application references Domain — if Domain references either, build fails.
**Why it happens:** .NET project references form a DAG. Domain referencing Application or Infrastructure creates a cycle.
**How to avoid:** IHueyTaskRunner in Domain has NO implementation. Domain is interface-only. Infrastructure implements the interface. This is the standard hexagonal approach.
**Warning signs:** `error CS0260: Missing partial modifier on declaration` or circular dependency errors during build.

### Pitfall 2: Wrong Using Statements After Move
**What goes wrong:** Code still imports `InsuranceManager.Application.Huey` after the interface moves.
**Why it happens:** Using statements in consuming classes not updated when files move.
**How to avoid:** Update `using InsuranceManager.Application.Huey;` → `using InsuranceManager.Domain.Ports;` in:
- `ProposalService.cs`
- `Program.cs`
**Warning signs:** `error CS0234: The type or namespace 'Huey' does not exist in namespace 'InsuranceManager.Application'`

### Pitfall 3: Class Name Collision
**What goes wrong:** Both old (Application) and new (Infrastructure) files exist with same class name.
**Why it happens:** Forgot to delete the original file during refactor.
**How to avoid:** Delete `Application/Huey/HueyTaskRunner.cs` as a separate step after creating the adapter.
**Warning signs:** `error CS0433: The type 'HueyTaskRunner' exists in both assemblies`

### Pitfall 4: Domain Having External Dependencies
**What goes wrong:** IHueyTaskRunner.cs uses types that require external packages (e.g., Microsoft.Extensions.Configuration).
**Why it happens:** Interface definition uses namespace from a package that Domain doesn't reference.
**How to avoid:** IHueyTaskRunner should only use types from Domain layer (entities, value objects). IConfiguration is used in the adapter, not the interface.
**Warning signs:** Domain project build fails with missing reference errors.

## Code Examples

### Existing Pattern: Repository Port & Adapter (Reference)
```csharp
// Domain/Ports/IProposalRepository.cs — interface only
public interface IProposalRepository
{
    Task<Proposal?> GetByIdAsync(Guid id, CancellationToken ct = default);
    // ... other methods
}

// Infrastructure/Adapters/ProposalRepository.cs — concrete implementation
public class ProposalRepository : IProposalRepository
{
    private readonly InsuranceDbContext _context;
    public ProposalRepository(InsuranceDbContext context) { ... }
    // ... implementation
}

// Program.cs — DI registration
builder.Services.AddScoped<IProposalRepository, ProposalRepository>();
```

### Phase 5: HueyTaskRunner Port & Adapter (New)
```csharp
// Domain/Ports/IHueyTaskRunner.cs — interface only
public interface IHueyTaskRunner
{
    Task EnqueueStatusChangeAsync(Guid proposalId, ProposalStatus newStatus, CancellationToken ct = default);
}

// Infrastructure/Huey/HueyTaskRunnerAdapter.cs — concrete implementation
public class HueyTaskRunnerAdapter : IHueyTaskRunner
{
    private readonly string _hueyDir;
    public HueyTaskRunnerAdapter(IConfiguration configuration) { ... }
    // ... implementation
}

// Program.cs — DI registration (UPDATE)
builder.Services.AddScoped<IHueyTaskRunner, HueyTaskRunnerAdapter>();
```

## State of the Art

| Old Approach | Current Approach | When Changed | Impact |
|--------------|------------------|--------------|--------|
| HueyTaskRunner in Application layer | HueyTaskRunnerAdapter in Infrastructure, port in Domain | v1.2 (Phase 5) | Enforces hexagonal architecture |
| Application knows about Huey specifics | Application knows only about IHueyTaskRunner port | v1.2 (Phase 5) | Decoupled, testable |

**Deprecated/outdated:**
- HueyTaskRunner in Application.Huey: Deprecated, will be removed in Phase 5

## Assumptions Log

> List all claims tagged `[ASSUMED]` in this research. The planner and discuss-phase use this
> section to identify decisions that need user confirmation before execution.

| # | Claim | Section | Risk if Wrong |
|---|-------|---------|---------------|
| A1 | Domain can reference ProposalStatus from Domain.ValueObjects | Architecture Patterns | Low — already used by other ports |
| A2 | IConfiguration injection pattern works in Infrastructure | Common Pitfalls | Low — already used in HueyTaskRunner |
| A3 | Namespace convention follows pattern: Domain.Ports, Infrastructure.Huey | Project Structure | Low — follows existing conventions |

**If this table is empty:** All claims in this research were verified or cited — no user confirmation needed.

## Open Questions

1. **Should HueyTaskRunner.cs in Application be deleted or just become the adapter?**
   - What we know: File currently contains both interface and implementation in Application/Huey/HueyTaskRunner.cs
   - What's unclear: Approach for split (move interface, move implementation, delete original)
   - Recommendation: Move interface content to Domain/Ports/IHueyTaskRunner.cs, move implementation to Infrastructure/Huey/HueyTaskRunnerAdapter.cs, then delete original file

## Environment Availability

Step 2.6: SKIPPED (no external dependencies identified — pure refactoring task)

## Validation Architecture

### Test Framework
| Property | Value |
|----------|-------|
| Framework | xUnit (existing) |
| Config file | `src/InsuranceManager.Tests/` |
| Quick run command | `dotnet test --no-build` |
| Full suite command | `dotnet test` |

### Phase Requirements → Test Map
| Req ID | Behavior | Test Type | Automated Command | File Exists? |
|--------|----------|-----------|-------------------|---------------|
| ARCH-01 | HueyTaskRunner moved to Infrastructure | Manual | Build verification only | — |
| ARCH-02 | IHueyTaskRunner exists in Domain/Ports | Manual | Build verification only | — |
| ARCH-03 | HueyTaskRunnerAdapter implements IHueyTaskRunner | Manual | Build verification only | — |

### Sampling Rate
- **Per task commit:** `dotnet build --no-restore` (fast verification)
- **Per wave merge:** `dotnet test` (full suite)
- **Phase gate:** Build and test green before `/gsd-verify-work`

### Wave 0 Gaps
None — no new test infrastructure needed. Existing tests verify existing behavior; refactoring doesn't change behavior.

## Security Domain

> Required when `security_enforcement` is enabled (absent = enabled). Omit only if explicitly `false` in config.

### Applicable ASVS Categories

| ASVS Category | Applies | Standard Control |
|---------------|---------|------------------|
| V4 Access Control | No | N/A |
| V5 Input Validation | No | N/A |

### Known Threat Patterns for Hexagonal Architecture

| Pattern | STRIDE | Standard Mitigation |
|---------|--------|---------------------|
| Infrastructure leak into Domain | Information Disclosure | Domain project has no external dependencies |
| Circular dependency | Denial of Service | Layered project references only |

**Security observation:** Phase 5 is a refactoring task — no new attack surface introduced. Moving code between layers with proper interface boundaries maintains security posture.

## Sources

### Primary (HIGH confidence)
- [VERIFIED: codebase inspection] - `src/InsuranceManager.Domain/Ports/IProposalRepository.cs` — existing port pattern
- [VERIFIED: codebase inspection] - `src/InsuranceManager.Application/Huey/HueyTaskRunner.cs` — current implementation to move
- [VERIFIED: codebase inspection] - `src/InsuranceManager.Application/Services/ProposalService.cs` — consumer of IHueyTaskRunner
- [VERIFIED: codebase inspection] - `src/InsuranceManager.Api/Program.cs` — DI registration

### Secondary (MEDIUM confidence)
- [ASSUMED] - Microsoft.Extensions.Configuration.Abstractions in Infrastructure — already in use via EF Core

### Tertiary (LOW confidence)
- [ASSUMED] - Hexagonal architecture best practices — standard .NET patterns

## Metadata

**Confidence breakdown:**
- Standard stack: HIGH - Uses existing patterns already in codebase
- Architecture: HIGH - Verified against existing codebase structure
- Pitfalls: HIGH - Based on common .NET refactoring issues, verified against known patterns

**Research date:** 2026-05-11
**Valid until:** 2026-06-10 (30 days for stable architecture pattern)