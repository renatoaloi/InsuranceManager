# Phase 6: Integration & Architecture Validation - Research

**Researched:** 2026-05-11
**Domain:** .NET Dependency Injection / Hexagonal Architecture Validation
**Confidence:** HIGH

## Summary

Phase 6 validates that the hexagonal architecture is properly wired and Domain layer has zero external dependencies. **The architecture is already fully implemented and all tests pass.** The only finding is a naming discrepancy: requirements reference `IHueyTaskRunner` but implementation uses `IQueueTaskAdapter`.

**Primary recommendation:** Phase 6 is essentially complete — run the architecture tests to confirm and optionally update REQUIREMENTS.md to reflect actual implementation naming.

---

## User Constraints (from CONTEXT.md)

*Note: Phase 6 has no CONTEXT.md as it's a validation/conclusion phase. The constraints come from ROADMAP.md and REQUIREMENTS.md:*

- **ARCH-04:** IHueyTaskRunner is registered in DI container and injected into consuming Application services
- **ARCH-05:** Domain layer has no references to Infrastructure or Application (zero external dependencies)

---

## Phase Requirements

| ID | Description | Research Support |
|----|-------------|------------------|
| ARCH-04 | IHueyTaskRunner is registered in DI and injected into Application services | Verified in Program.cs (line 25) and ProposalService.cs (line 14) — DI wiring is complete. Actual implementation uses `IQueueTaskAdapter` (not `IHueyTaskRunner`). |
| ARCH-05 | Domain layer has no references to Infrastructure or Application | Verified by 6 tests in DomainLayerIsolationTests.cs + successful `dotnet build`. Domain has zero project references and zero package references. |

---

## Architectural Responsibility Map

| Capability | Primary Tier | Secondary Tier | Rationale |
|------------|-------------|----------------|-----------|
| DI Registration | API (Program.cs) | — | Program.cs wires all services, including IQueueTaskAdapter → QueueTaskRunnerAdapter |
| Port Definition | Domain (IQueueTaskAdapter) | — | Domain layer owns port interfaces per hexagonal architecture |
| Adapter Implementation | Infrastructure (QueueTaskRunnerAdapter) | — | Infrastructure implements ports, handles Huey integration details |
| Architecture Validation | Test Infrastructure | — | Architecture tests verify isolation and DI wiring |

---

## Standard Stack

| Library | Version | Purpose | Why Standard |
|---------|---------|---------|--------------|
| Microsoft.Extensions.DependencyInjection | (bundled with .NET 10) | DI container for .NET | Standard .NET pattern for IoC |
| Microsoft.Extensions.Configuration | (bundled with .NET 10) | Configuration injection | Already used in QueueTaskRunnerAdapter |
| xUnit | Latest (via test project) | Unit testing framework | Standard .NET test framework |
| Moq | Latest | Mocking for tests | Standard .NET mocking |

---

## Current Architecture State

### DI Wiring (ARCH-04)

**DI Registration in Program.cs:**
```csharp
// Line 25 in src/InsuranceManager.Api/Program.cs
builder.Services.AddScoped<IQueueTaskAdapter, QueueTaskRunnerAdapter>();
```

**Consuming Service Injection in ProposalService.cs:**
```csharp
// Lines 12-14 in src/InsuranceManager.Application/Services/ProposalService.cs
private readonly IQueueTaskAdapter? _hueyTaskRunner;

public ProposalService(
    IProposalRepository repository,
    IProposalReadAdapter readAdapter,
    IQueueTaskAdapter? hueyTaskRunner = null)
```

**Verification:** DI wiring is complete and functional. [VERIFIED: codebase scan]

### Domain Layer Isolation (ARCH-05)

**Domain.csproj - NO references:**
```xml
<!-- src/InsuranceManager.Domain/InsuranceManager.Domain.csproj -->
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
  </PropertyGroup>
  <!-- No <ItemGroup> with ProjectReference or PackageReference -->
</Project>
```

**Using statements scan in Domain layer:**
- `grep "using InsuranceManager.Infrastructure" src/InsuranceManager.Domain/**/*.cs` → 0 matches [VERIFIED: grep tool]
- `grep "using InsuranceManager.Application" src/InsuranceManager.Domain/**/*.cs` → 0 matches [VERIFIED: grep tool]

**Build verification:**
```
dotnet build src/InsuranceManager.Domain/InsuranceManager.Domain.csproj
→ Compilação com êxito. 0 Erro(s)
```

[VERIFIED: dotnet build output]

---

## Architecture Tests

### Test Coverage

**Test project:** `tests/InsuranceManager.Architecture.Tests/`

**Test file 1: DomainLayerIsolationTests.cs**
| Test | What It Validates | Status |
|------|-------------------|--------|
| DomainProject_HasNoProjectReferences | No `<ProjectReference>` in Domain.csproj | ✅ PASS |
| DomainProject_HasNoExternalPackageReferences | No `<PackageReference>` in Domain.csproj | ✅ PASS |
| DomainProject_ContainsOnlyCoreFiles | Files only in Entities, Ports, ValueObjects, Events | ✅ PASS |
| DomainFiles_HaveNoInfrastructureUsings | No `using InsuranceManager.Infrastructure` in .cs files | ✅ PASS |
| DomainFiles_HaveNoExternalInfrastructureDependencies | No EF Core or Extensions references | ✅ PASS |

**Test file 2: PortAdapterPatternTests.cs**
| Test | What It Validates | Status |
|------|-------------------|--------|
| PortInterface_ExistsInDomainLayer | IQueueTaskAdapter in Domain.Ports namespace | ✅ PASS |
| AdapterImplementation_ExistsInInfrastructureLayer | QueueTaskRunnerAdapter in Infrastructure.Queue | ✅ PASS |
| Adapter_ImplementsPortContract | Adapter implements IQueueTaskAdapter | ✅ PASS |
| ApplicationService_UsesPortAbstraction | ProposalService accepts IQueueTaskAdapter (not concrete) | ✅ PASS |
| PortInterface_DefinesCorrectContract | Method signature: (Guid, ProposalStatus, CancellationToken) | ✅ PASS |
| AllPorts_AreInDomainLayer | All port interfaces in Domain layer | ✅ PASS |

**Test run result:**
```
dotnet test --filter "FullyQualifiedName~Architecture.Tests"
→ Aprovado! Com falha: 0, Aprovado: 12, Total: 12
```

[VERIFIED: dotnet test output]

---

## Naming Discrepancy

### What the Requirements Say (REQUIREMENTS.md)
- **ARCH-01:** Move HueyTaskRunner from Application to Infrastructure
- **ARCH-02:** Create `IHueyTaskRunner` port interface in Domain layer
- **ARCH-03:** Implement `HueyTaskRunnerAdapter` in Infrastructure
- **ARCH-04:** Inject `IHueyTaskRunner` via DI in Application services

### What the Code Actually Uses
- Port: `IQueueTaskAdapter` (Domain/Ports/IQueueTaskAdapter.cs)
- Adapter: `QueueTaskRunnerAdapter` (Infrastructure/Queue/QueueTaskRunnerAdapter.cs)
- Namespace: `Infrastructure.Queue` (not `Infrastructure.Huey`)

### Analysis
This appears to be a Phase 5 decision to use more descriptive naming:
- `IQueueTaskAdapter` is more generic than `IHueyTaskRunner` (port doesn't know about Huey)
- `QueueTaskRunnerAdapter` follows standard adapter naming convention

The implementation is **correct** — it's the requirements that need alignment.

---

## Recommendations

### Option 1: Requirements Alignment (Recommended)
Update `.planning/REQUIREMENTS.md` to reflect actual naming:
- Replace `IHueyTaskRunner` with `IQueueTaskAdapter`
- Replace `HueyTaskRunnerAdapter` with `QueueTaskRunnerAdapter`

This keeps implementation as-is and updates documentation.

### Option 2: Implementation Rename
If the original naming (`IHueyTaskRunner`) is preferred:
- Rename `IQueueTaskAdapter` → `IHueyTaskRunner`
- Rename `QueueTaskRunnerAdapter` → `HueyTaskRunnerAdapter`
- Update DI registration and all references

This is more work and provides no architectural benefit.

### Recommendation
**Use Option 1** — the current naming is better (port abstraction doesn't know about Huey specifics). Update REQUIREMENTS.md to match.

---

## What's Needed for Phase 6

| Requirement | Current State | Action Needed |
|-------------|---------------|---------------|
| ARCH-04: DI wired | ✅ Complete | Verify tests pass |
| ARCH-05: Domain isolation | ✅ Complete | Verify tests pass |
| Update REQUIREMENTS.md | ⚠️ Naming mismatch | Update to reflect `IQueueTaskAdapter` |

**Estimated Phase 6 effort:** Minimal — primarily validation and documentation update.

---

## Common Pitfalls

### Pitfall 1: Nullability Mismatch
**What goes wrong:** ProposalService accepts `IQueueTaskAdapter?` (nullable), but DI always provides it. The `?` allows null for optional behavior but could mask configuration errors.

**How to avoid:** Current implementation handles this correctly with null-conditional usage in `EnqueueStatusChangeAsync`.

### Pitfall 2: Adapter Discovery
**What goes wrong:** Adding new adapters but forgetting DI registration in Program.cs causes runtime null references.

**Prevention:** The architecture tests verify the DI registration exists, but there are no runtime tests confirming the adapter resolves correctly.

---

## Open Questions

1. **Should REQUIREMENTS.md be updated to match actual implementation naming?**
   - What we know: Implementation uses `IQueueTaskAdapter`, requirements say `IHueyTaskRunner`
   - What's unclear: Was this an intentional rename or an oversight?
   - Recommendation: Update requirements to match — current naming is architecturally superior

2. **Does Phase 6 need any additional verification beyond existing tests?**
   - What we know: 12 architecture tests cover all requirements
   - What's unclear: Is there value in adding integration tests that verify DI resolution at runtime?
   - Recommendation: Current test coverage is sufficient for v1.2 milestone

---

## Environment Availability

| Dependency | Required By | Available | Version | Fallback |
|------------|------------|-----------|---------|----------|
| dotnet | Build/test | ✓ | 10.0.x | — |
| xUnit test runner | Architecture tests | ✓ | (via test project) | — |
| Moq | Mocking in tests | ✓ | (via test project) | — |

**All dependencies available** — no action needed.

---

## Sources

### Primary (HIGH confidence)
- `src/InsuranceManager.Domain/InsuranceManager.Domain.csproj` — Verified zero project/package references
- `src/InsuranceManager.Api/Program.cs` — Verified DI registration line 25
- `src/InsuranceManager.Application/Services/ProposalService.cs` — Verified IQueueTaskAdapter injection
- `tests/InsuranceManager.Architecture.Tests/` — Verified all 12 tests pass

### Secondary (HIGH confidence)
- `grep` tool output — Verified zero forbidden using statements in Domain layer
- `dotnet build` output — Verified Domain project compiles successfully
- `dotnet test` output — Verified 12/12 architecture tests pass

---

## Metadata

**Confidence breakdown:**
- Standard stack: HIGH — Standard .NET 10 / DI patterns
- Architecture: HIGH — Hexagonal architecture correctly implemented
- Pitfalls: HIGH — Known patterns documented

**Research date:** 2026-05-11
**Valid until:** 90 days (architecture stable, no pending changes)