# Phase 2: Status Lifecycle + Auth - Research

**Researched:** 2026-05-09
**Domain:** Huey async queue integration, API Key authentication middleware, CQRS read adapters, Proposal state machine
**Confidence:** MEDIUM-HIGH

## Summary

Phase 2 adds three orthogonal capabilities: async status transitions via Huey, dedicated read adapters (CQRS-02/03), and API Key authentication. The core challenge is bridging Python's Huey library with .NET on Windows. Key finding: use `Process.Start` to spawn a Python Huey consumer process from .NET, with FileHuey as the filesystem broker for Windows+Docker compatibility. The state machine for proposals already exists in the domain entity (`Approve()`/`Reject()`) — Phase 2 exposes these through an async queue. Read adapters follow the same port/interface pattern as write adapters from Phase 1. API Key auth uses ASP.NET Core middleware extracting `X-API-Key` header against an `appsettings.json` key.

## User Constraints (from CONTEXT.md)

> No CONTEXT.md exists for Phase 2 — no prior user decisions constrain this phase.

## Architectural Responsibility Map

| Capability | Primary Tier | Secondary Tier | Rationale |
|------------|--------------|----------------|-----------|
| Proposal status transitions | API + Huey Worker | Domain entity | API receives request, queues to Huey; Huey worker calls domain entity methods |
| State machine validation | Domain | — | `Proposal.Approve()` / `Proposal.Reject()` enforce valid transitions |
| API Key authentication | API (Middleware) | — | All endpoints pass through auth middleware |
| Read projections | Infrastructure (Adapters) | — | Read adapters implement query interfaces in Infrastructure layer |
| Huey queue submission | API (Application) | — | `ProposalService.ChangeStatusAsync()` enqueues Huey task |
| Huey task execution | Python process | — | Huey worker (separate process) executes queued tasks |

## Standard Stack

### Core
| Library | Version | Purpose | Why Standard |
|--------|---------|---------|--------------|
| Python Huey | 3.0.0 | Async task queue with FileHuey broker | Project constraint; lightweight, filesystem broker works on Windows |
| Python (embedded via Process) | 3.12 | Spawn Huey worker from .NET | Windows development compatibility |

### .NET / ASP.NET Core
| Library | Version | Purpose | Why Standard |
|--------|---------|---------|--------------|
| ASP.NET Core Middleware | (built-in) | API Key auth pipeline | Standard .NET pattern for request filtering |
| EF Core SQLite | 10.0.0 | Persistence | Existing infrastructure from Phase 1 |

### Alternatives Considered
| Instead of | Could Use | Tradeoff |
|------------|-----------|----------|
| Process.Start Huey consumer | .NET-native queue (Channels, System.Threading.Tasks) | Violates PROJECT.md constraint "Huey"; would require rewriting Phase 3 |
| Redis Huey | FileHuey | Redis requires extra service; FileHuey works on Windows + Docker |
| NuGet `AspNetCore.Authentication.ApiKey` | Custom middleware | Third-party package adds dep; custom middleware is 15 lines and no dep |
| IAuthorizationFilter attribute | Global middleware | Global middleware is simpler (AUTH-01: "all endpoints") |

## Architecture Patterns

### System Architecture Diagram

```
Client Request (X-API-Key header)
        │
        ▼
┌───────────────────────────────┐
│  ASP.NET Core API             │
│  ┌─────────────────────────┐  │
│  │ ApiKeyMiddleware        │──┼── 401 if missing/invalid
│  └─────────────────────────┘  │
│  ProposalsController          │
│    POST /api/proposals/{id}/status
│        │                       │
│        ▼                       │
│  ProposalService.ChangeStatusAsync()
│        │ enqueues to           │
│        ▼ queue file            │
│  Huey queue (FileHuey)         │
└──────┬────────────────────────┘
       │ reads from
       │ filesystem queue
       ▼
┌───────────────────────────────┐
│  Python Huey Consumer Worker  │
│  (huey_consumer.py process)   │
│    - reads queued task        │
│    - calls domain via         │
│      HttpClient to /internal/  │
│      or direct DB update      │
└───────────────────────────────┘
       │
       ▼
┌───────────────────────────────┐
│  SQLite Database              │
│  Proposal.Approve()/Reject()  │
│  → valid transitions only     │
└───────────────────────────────┘
```

### Recommended Project Structure

```
src/
├── InsuranceManager.Api/
│   ├── Controllers/
│   ├── DTOs/
│   ├── Middleware/
│   │   └── ApiKeyMiddleware.cs        ← NEW
│   └── appsettings.json               ← MODIFIED (add ApiKey)
├── InsuranceManager.Application/
│   ├── Commands/
│   │   └── ChangeProposalStatusCommand.cs  ← NEW
│   ├── Queries/                        ← NEW
│   │   ├── IProposalReadAdapter.cs     ← NEW (port interface)
│   │   └── ProposalReadModel.cs        ← NEW (dto-style read model)
│   ├── Services/
│   │   ├── ProposalService.cs          ← MODIFIED (add status change + queue)
│   │   └── ProposalStatusQueueService.cs ← NEW (Huey enqueue wrapper)
│   └── Huey/                           ← NEW
│       └── HueyTaskRunner.cs           ← NEW (.NET → Huey bridge)
├── InsuranceManager.Infrastructure/
│   ├── Persistence/
│   └── Adapters/
│       ├── ProposalRepository.cs       ← READ (existing, unchanged)
│       └── ReadAdapters/               ← NEW
│           ├── ProposalReadAdapter.cs  ← NEW (CQRS-02/03)
│           └── PolicyReadAdapter.cs     ← NEW (CQRS-03)
├── InsuranceManager.Domain/
│   ├── Entities/
│   │   ├── Proposal.cs                 ← READ (already has Approve/Reject)
│   │   └── Policy.cs
│   ├── Ports/
│   │   ├── IProposalRepository.cs       ← READ (already has UpdateAsync)
│   │   └── IProposalReadAdapter.cs     ← NEW (CQRS read port)
│   └── ValueObjects/
│       └── ProposalStatus.cs            ← READ
```

### Pattern 1: API Key Middleware (ASP.NET Core)
**What:** Global middleware that intercepts every request, checks for `X-API-Key` header, validates against configured key.
**When to use:** AUTH-01: all endpoints require API Key authentication.
**Example:**
```csharp
// src/InsuranceManager.Api/Middleware/ApiKeyMiddleware.cs
public class ApiKeyMiddleware
{
    private readonly RequestDelegate _next;
    private const string ApiKeyHeaderName = "X-API-Key";

    public ApiKeyMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context, IConfiguration configuration)
    {
        // Skip Swagger endpoints
        if (context.Request.Path.StartsWithSegments("/swagger"))
        {
            await _next(context);
            return;
        }

        if (!context.Request.Headers.TryGetValue(ApiKeyHeaderName, out var extractedApiKey))
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsJsonAsync(new { error = "API Key is missing." });
            return;
        }

        var apiKey = configuration.GetValue<string>("ApiKey");
        if (!string.Equals(apiKey, extractedApiKey, StringComparison.Ordinal))
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsJsonAsync(new { error = "Invalid API Key." });
            return;
        }

        await _next(context);
    }
}
```
Register in `Program.cs`:
```csharp
app.UseMiddleware<ApiKeyMiddleware>();
```
appsettings.json addition:
```json
{
  "ApiKey": "your-secret-api-key-here"
}
```

### Pattern 2: Huey Enqueue from .NET (Process.Start)
**What:** .NET spawns a Python Huey consumer process and submits tasks by writing to the queue filesystem.
**When to use:** PROP-04: async status change requests processed via Huey.
**Mechanism:**
1. Huey's FileHuey stores queue data as pickle files in a directory (e.g., `./huey_data/queue`)
2. Python's `huey.api` provides `Huey.enqueue()` function
3. .NET calls Python via `Process.Start("python", "-c", pythonCode)` to enqueue tasks
4. A persistent Huey consumer process (`huey_consumer.py ./huey_data/huey_config.py`) runs in background and processes tasks

**Enqueue task from .NET:**
```csharp
// src/InsuranceManager.Application/Huey/HueyTaskRunner.cs
public class HueyTaskRunner
{
    private readonly string _hueyDir;
    private readonly string _pythonPath;

    public HueyTaskRunner(IConfiguration configuration)
    {
        _hueyDir = configuration.GetValue<string>("Huey:QueuePath") ?? "./huey_data";
        _pythonPath = configuration.GetValue<string>("Python:Path") ?? "python";
    }

    public async Task EnqueueStatusChangeAsync(Guid proposalId, string newStatus, CancellationToken ct = default)
    {
        var pythonCode = $@"
import sys
sys.path.insert(0, 'src/InsuranceManager.Application/Huey')
from huey_config import huey
@huey.task()
def process_status_change(proposal_id, status):
    # Status change task - Huey will execute this asynchronously
    pass
huey.enqueue(process_status_change('{proposalId}', '{newStatus}'))
";
        var psi = new System.Diagnostics.ProcessStartInfo
        {
            FileName = _pythonPath,
            Arguments = $"-c \"{pythonCode.Replace("\"", "\\\"")}\"",
            WorkingDirectory = Directory.GetCurrentDirectory(),
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = System.Diagnostics.Process.Start(psi);
        await process.WaitForExitAsync(ct);
    }
}
```

**Huey config file (Python):**
```python
# src/InsuranceManager.Application/Huey/huey_config.py
import os
from huey import Huey, FileHuey

huey = FileHuey('insurance_huey', path=os.path.join(os.path.dirname(__file__), '../../huey_data'))
```

### Pattern 3: CQRS Read Adapters (following Phase 1 pattern)
**What:** Separate read adapter interfaces (ports) in Domain, implementations (adapters) in Infrastructure.
**When to use:** CQRS-02, CQRS-03: read operations use dedicated read adapters with optimized projections.
**Example:**

Domain port (read interface):
```csharp
// src/InsuranceManager.Domain/Ports/IProposalReadAdapter.cs
using InsuranceManager.Domain.ValueObjects;

namespace InsuranceManager.Domain.Ports;

public record ProposalListItem(
    Guid Id,
    string ClientName,
    CoverageType CoverageType,
    ProposalStatus Status,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public interface IProposalReadAdapter
{
    Task<IReadOnlyList<ProposalListItem>> GetAllAsync(
        ProposalStatus? status = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken ct = default);

    Task<ProposalListItem?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task<int> GetCountByStatusAsync(ProposalStatus status, CancellationToken ct = default);
}
```

Infrastructure adapter (implementation):
```csharp
// src/InsuranceManager.Infrastructure/Adapters/ReadAdapters/ProposalReadAdapter.cs
using Microsoft.EntityFrameworkCore;
using InsuranceManager.Domain.Ports;
using InsuranceManager.Infrastructure.Persistence;

namespace InsuranceManager.Infrastructure.Adapters.ReadAdapters;

public class ProposalReadAdapter : IProposalReadAdapter
{
    private readonly InsuranceDbContext _ctx;

    public ProposalReadAdapter(InsuranceDbContext ctx) => _ctx = ctx;

    public async Task<IReadOnlyList<ProposalListItem>> GetAllAsync(
        ProposalStatus? status = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken ct = default)
    {
        var query = _ctx.Proposals.AsQueryable();

        if (status.HasValue)
            query = query.Where(p => p.Status == status.Value);

        if (fromDate.HasValue)
            query = query.Where(p => p.CreatedAt >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(p => p.CreatedAt <= toDate.Value);

        var results = await query
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new ProposalListItem(
                p.Id,
                p.ClientName,
                p.CoverageType,
                p.Status,
                p.CreatedAt,
                p.UpdatedAt))
            .ToListAsync(ct);

        return results;
    }

    public async Task<ProposalListItem?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _ctx.Proposals
            .Where(p => p.Id == id)
            .Select(p => new ProposalListItem(
                p.Id, p.ClientName, p.CoverageType, p.Status, p.CreatedAt, p.UpdatedAt))
            .FirstOrDefaultAsync(ct);
    }

    public async Task<int> GetCountByStatusAsync(ProposalStatus status, CancellationToken ct = default)
        => await _ctx.Proposals.CountAsync(p => p.Status == status, ct);
}
```

## Don't Hand-Roll

| Problem | Don't Build | Use Instead | Why |
|---------|-----------|-------------|-----|
| Task queue | Build a custom .NET queue from scratch | Huey with FileHuey | PROJECT.md constrains to Huey; FileHuey provides persistence, retries, scheduling, priorities |
| File locking | Build custom file-based queue | Huey FileHuey | FileHuey uses exclusive file locks — handles concurrent access correctly |
| Authentication | Build a custom auth framework | ASP.NET Core middleware | Built-in pipeline, DI integration, middleware composition |

**Key insight:** The project explicitly chose Huey for its filesystem broker to achieve Windows+Docker compatibility without Redis. Trying to replace it with a .NET-native solution breaks Phase 3 requirements (INFR-01/02/03: Huey worker in Docker).

## Runtime State Inventory

> Skip — this is not a rename/refactor/migration phase. No runtime state changes apply.

## Common Pitfalls

### Pitfall 1: Huey Consumer Not Running
**What goes wrong:** Tasks are enqueued but never executed — status changes silently fail.
**Why it happens:** Huey consumer is a separate Python process that must be started and kept running.
**How to avoid:** Start Huey consumer as a background process alongside the API. Use `Process.Start` with `UseShellExecute = false` and capture output for logging.
**Warning signs:** Tasks accumulate in `./huey_data/queue/` directory but never get processed.

### Pitfall 2: FileHuey Not Compatible with Docker CIFS Mounts
**What goes wrong:** FileHuey tasks process fine on local Windows but fail silently in Docker container when using CIFS/Windows network shares.
**Why it happens:** FileHuey relies on exclusive file locks which don't work reliably over CIFS.
**How to avoid:** Use Docker volume mounts (bind mounts) for the Huey queue directory in Phase 3. Do not use CIFS shares for `./huey_data/`.
**Warning signs:** Tasks queue but don't process in Docker; Docker logs show "file busy" or lock errors.

### Pitfall 3: API Key in Source Control
**What goes wrong:** API key committed to git, rotated keys break production.
**Why it happens:** Default appsettings.json added to git, developer forgets to use `.gitignore` or environment variables.
**How to avoid:** Store API key in environment variable `API_KEY`. Read from `IConfiguration` which automatically falls back to env vars. Add `appsettings.*.json` to `.gitignore` for secrets.

### Pitfall 4: Mixing Read/Write in Same Repository
**What goes wrong:** Read queries pollute write adapter, violating CQRS separation.
**Why it happens:** Phase 1 `ProposalRepository.GetAllAsync()` mixes read logic with write adapter. Phase 2 should add dedicated read adapters.
**How to avoid:** Create separate `IProposalReadAdapter` port in Domain, implement in `ReadAdapters/` folder in Infrastructure. Read adapters return `ProposalListItem` records, not entity objects.

## Code Examples

### Proposal Status Change Command (CQRS)
```csharp
// src/InsuranceManager.Application/Commands/ChangeProposalStatusCommand.cs
namespace InsuranceManager.Application.Commands;

public record ChangeProposalStatusCommand(Guid ProposalId, ProposalStatus NewStatus);

// src/InsuranceManager.Application/Services/ProposalService.cs — add method:
public async Task EnqueueStatusChangeAsync(ChangeProposalStatusCommand command, CancellationToken ct = default)
{
    // Validate transition before enqueueing
    var proposal = await _repository.GetByIdAsync(command.ProposalId, ct)
        ?? throw new InvalidOperationException("Proposal not found");

    if (!proposal.CanTransitionTo(command.NewStatus))
        throw new InvalidOperationException($"Invalid transition from {proposal.Status} to {command.NewStatus}");

    // Enqueue to Huey
    await _hueyTaskRunner.EnqueueStatusChangeAsync(command.ProposalId, command.NewStatus.ToString(), ct);
}
```

### Proposal State Machine (Domain Enhancement)
```csharp
// src/InsuranceManager.Domain/Entities/Proposal.cs — add state validation:
public bool CanTransitionTo(ProposalStatus targetStatus)
{
    return (Status, targetStatus) switch
    {
        (ProposalStatus.EmAnalise, ProposalStatus.Aprovada) => true,
        (ProposalStatus.EmAnalise, ProposalStatus.Recusada) => true,
        _ => false
    };
}
```

### Status Change Endpoint
```csharp
// src/InsuranceManager.Api/Controllers/ProposalsController.cs — add endpoint:
[HttpPost("{id:guid}/status")]
public async Task<ActionResult> ChangeStatus(
    Guid id,
    [FromBody] ChangeProposalStatusDto dto,
    CancellationToken ct)
{
    var command = new ChangeProposalStatusCommand(id, dto.Status);
    await _proposalService.EnqueueStatusChangeAsync(command, ct);
    return Accepted(new { message = "Status change request queued" });
}
```

## State of the Art

| Old Approach | Current Approach | When Changed | Impact |
|--------------|-----------------|--------------|--------|
| Status change synchronous (direct DB update) | Async via Huey queue | Phase 2 | API responds immediately (202), actual processing is async |
| Single repository for all access | Separate read/write adapters | Phase 2 (CQRS-02/03) | Read queries can be optimized independently |
| No authentication | API Key via middleware | Phase 2 | All endpoints protected |

**Deprecated/outdated:**
- None for this phase

## Assumptions Log

| # | Claim | Section | Risk if Wrong |
|---|-------|---------|---------------|
| A1 | Huey 3.0.0 is available via pip | Environment Availability | If Huey is not installed, plans must include `pip install huey` |
| A2 | Python process can spawn from .NET on Windows | Huey Integration | If Python spawning is blocked, need alternative mechanism |
| A3 | FileHuey exclusive locks handle concurrent access correctly | Huey Integration | If FileHuey has race conditions on Windows, need SqliteHuey instead |
| A4 | Huey consumer process can access same SQLite DB as .NET API | Huey Integration | If SQLite is locked, Huey worker can't update proposal status |

**If this table is empty:** All claims were verified or cited — no user confirmation needed.

## Open Questions

1. **How does the Huey consumer update proposal status?**
   - What we know: Huey is Python, SQLite is EF Core. Consumer needs to update the DB.
   - Options: (a) Huey consumer calls a private HTTP endpoint on the API (`/internal/status`), (b) Huey consumer imports .NET assemblies via Python.NET (pythonnet), (c) Huey consumer writes to DB directly using Python sqlite3. Option (a) is simplest and most maintainable.
   - **Recommendation:** Use private HTTP endpoint `/internal/status` — keeps all DB access through EF Core.

2. **How to manage Huey consumer process lifecycle in Phase 2?**
   - What we know: Consumer must run as a separate process. In Phase 3 it runs in Docker.
   - Options: (a) Start consumer as background thread in the API process, (b) Spawn consumer via `Process.Start` and manage via `IHostedService`, (c) Developer starts consumer manually.
   - **Recommendation:** Use `BackgroundService` (`IHostedService`) that spawns and monitors the Python Huey consumer process — ensures it restarts if it crashes.

3. **How to expose the `IProposalReadAdapter` in DI?**
   - What we know: Phase 1 uses `IProposalRepository` (write adapter). Phase 2 adds `IProposalReadAdapter` (read adapter).
   - Both map to the same `InsuranceDbContext`. Need to ensure `DbContext` lifetime is correct.
   - **Recommendation:** Register `IProposalReadAdapter` as scoped, same as existing repositories.

## Environment Availability

| Dependency | Required By | Available | Version | Fallback |
|------------|------------|-----------|---------|----------|
| Python | Huey task queue | ✓ | 3.12.7 | — |
| pip | Installing Huey package | ✓ | 25.3 | — |
| Huey package | Async task processing | ✗ | — | `pip install huey` |
| Node.js | Not used in Phase 2 | ✓ | v24.14.1 | — |
| npm | Not used in Phase 2 | ✓ | 11.10.1 | — |

**Missing dependencies with fallback:**
- Huey not installed → `pip install huey` in setup steps

**Missing dependencies with no fallback:**
- None identified for Phase 2

## Security Domain

### Applicable ASVS Categories

| ASVS Category | Applies | Standard Control |
|---------------|---------|-----------------|
| V2 Authentication | yes | API Key via `X-API-Key` header; constant-time comparison with `StringComparison.Ordinal` |
| V3 Session Management | no | API Key auth is stateless; no sessions |
| V4 Access Control | no | Single API Key for all operations; no role-based access in Phase 2 |
| V5 Input Validation | yes | ASP.NET Core model binding + proposal status enum validation |
| V6 Cryptography | no | API Key stored in plain text in config (acceptable for internal service-to-service) |

### Known Threat Patterns for ASP.NET Core + Huey

| Pattern | STRIDE | Standard Mitigation |
|---------|--------|---------------------|
| API Key brute force | Denial | API Key must be sufficiently long (32+ chars); reject after N failed attempts (not implemented in Phase 2 — flag for Phase 3) |
| API Key in logs | Information Disclosure | `X-API-Key` header value should not be logged; ASP.NET Core does not log headers by default |
| Huey task injection | Tampering | Tasks are generated server-side only; .NET never passes raw user input to Huey |
| Status transition race condition | Tampering | Huey processes tasks sequentially per queue; FileHuey exclusive lock prevents concurrent status changes |

## Sources

### Primary (HIGH confidence)
- [Huey 3.0.0 documentation](https://huey.readthedocs.io/en/stable/) - FileHuey, task API, immediate mode
- [Huey GitHub repository](https://github.com/coleifer/huey) - latest version, FileHuey implementation

### Secondary (MEDIUM confidence)
- [C# Corner: Securing ASP.NET Core APIs with API Keys](https://www.c-sharpcorner.com/article/securing-asp-net-core-apis-with-api-keys/) - middleware pattern
- [Murat Süzen: API Key Authorization with Middleware](https://muratsuzen.github.io/en/posts/using-api-key-authorization-with-middleware-and-attribute-on-asp-net-core-web-api/) - middleware + Swagger integration

### Tertiary (LOW confidence)
- WebSearch for "Huey Python Windows .NET integration" - no direct hits; cross-validated via Huey FileHuey docs and Process.Start .NET docs

## Metadata

**Confidence breakdown:**
- Standard Stack: MEDIUM - Huey is confirmed as project constraint, but .NET→Python bridge via Process.Start is an assumption not verified in a live environment
- Architecture: HIGH - patterns follow Phase 1 conventions, CQRS read adapters are established pattern
- Pitfalls: MEDIUM - FileHuey on Windows and Docker CIFS issues are documented in Huey GitHub issues

**Research date:** 2026-05-09
**Valid until:** 2026-06-08 (30 days — stable domain)
