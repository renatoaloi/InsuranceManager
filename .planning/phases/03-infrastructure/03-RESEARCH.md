# Phase 3: Infrastructure - Research

**Researched:** 2026-05-09
**Domain:** Docker containerization, Huey worker orchestration, filesystem broker compatibility
**Confidence:** HIGH

## Summary

Phase 3 containerizes the .NET API and Huey worker for deployment. Key findings: use multi-stage Docker builds for .NET (reduce image from ~2GB to ~200MB), create a separate Python container for Huey worker using the same `huey_data` directory via bind mount, and ensure FileHuey uses bind mounts (not CIFS) for proper file locking. Health endpoints should be implemented at `/health` (liveness) and `/ready` (readiness) to support OBS-01/OBS-02 requirements in future phases. API Key must be passed via environment variable, never baked into Docker images.

**Primary recommendation:** Use multi-stage .NET 10 Dockerfile + Python slim container for Huey worker with shared bind mount for `./huey_data/` directory.

## User Constraints

> No CONTEXT.md exists for Phase 3 — no prior user decisions constrain this phase.

## Architectural Responsibility Map

| Capability | Primary Tier | Secondary Tier | Rationale |
|------------|--------------|----------------|-----------|
| API container | Docker (API container) | — | ASP.NET Core runs in Linux container, exposes port 5000/8080 |
| Huey worker container | Docker (Worker container) | — | Python process runs in separate container, consumes queue |
| Filesystem broker | Shared volume | API + Worker | `./huey_data/` directory shared between API (enqueue) and Worker (dequeue) |
| Health endpoints | API container | — | ASP.NET Core health checks return 200 when process is healthy |
| API Key config | Environment | — | Passed via Docker environment, read by middleware |

## Standard Stack

### Core
| Library | Version | Purpose | Why Standard |
|--------|---------|---------|--------------|
| .NET SDK | 10.0 | Build the API | Required for compiling ASP.NET Core application |
| ASP.NET Core Runtime | 10.0 | Run the API | Lightweight runtime for deployed container |
| Python | 3.12-slim | Run Huey worker | Slim variant reduces image size, version matches local dev |
| Huey | 3.0.0 | Async task queue | Project constraint from Phase 2 |

### Supporting
| Library | Version | Purpose | When to Use |
|---------|---------|---------|-------------|
| requests | (latest) | HTTP calls from Huey worker | Huey worker calls API internal endpoint |
| ASP.NET Core HealthChecks | (built-in) | Health endpoints | Enables /health and /ready endpoints |

### Alternatives Considered
| Instead of | Could Use | Tradeoff |
|------------|-----------|----------|
| Full .NET image | aspnet:10.0 | Slim image reduces attack surface, faster pulls |
| Redis for Huey | FileHuey | Redis requires extra service; FileHuey works on Windows + Docker |
| Single container for API + Huey | Separate containers | Separate containers allow independent scaling and restart |
| Named Docker volumes | Bind mounts | Named volumes abstract path; bind mounts match local dev structure |

**Installation:**
```bash
# .NET (handled in Dockerfile via mcr.microsoft.com/dotnet/sdk:10.0)
# Python with Huey
pip install huey requests
```

## Architecture Patterns

### System Architecture Diagram

```
┌─────────────────────────────────────────────────────────────────────┐
│                     docker-compose.yml                              │
│                                                                     │
│  ┌──────────────────────┐     ┌──────────────────────┐             │
│  │   insurance-api     │     │   insurance-huey   │             │
│  │   .NET 10 Container  │     │   Python Container  │             │
│  │                      │     │                      │             │
│  │  ASP.NET Core API    │     │  Huey Consumer      │             │
│  │  - /api/proposals    │     │  - Reads queue      │             │
│  │  - /api/policies     │     │  - Calls API        │             │
│  │  - /health           │     │  - Processes tasks  │             │
│  │  - /ready            │     │                      │             │
│  └──────────┬───────────┘     └──────────┬───────────┘             │
│             │                             │                         │
│             │     ┌───────────────┐       │                         │
│             │     │ huey_data/    │◄──────┘                         │
│             │     │ (bind mount)  │    Shared queue directory      │
│             │     └───────────────┘                                 │
│             │           │                                          │
│             ▼           ▼                                          │
│  ┌─────────────────────────────────────┐                           │
│  │  Host filesystem: ./huey_data/      │                           │
│  │  - queue/                            │                           │
│  │  - schedule.json                     │                           │
│  └─────────────────────────────────────┘                           │
└─────────────────────────────────────────────────────────────────────┘
```

### Recommended Project Structure

```
src/
├── InsuranceManager.Api/
│   ├── Dockerfile                    # Multi-stage .NET 10 build
│   └── ...
├── InsuranceManager.Application/
│   ├── Huey/
│   │   ├── huey_config.py            # FileHuey configuration
│   │   ├── huey_consumer.py          # Consumer worker script
│   │   └── requirements.txt          # Python dependencies (NEW)
│   └── ...
├── docker-compose.yml                 # API + Huey services
└── huey_data/                        # Queue directory (created at runtime)
```

### Pattern 1: Multi-Stage .NET 10 Dockerfile
**What:** Build .NET application in one stage, publish to minimal runtime stage.
**When to use:** INFR-01: Docker container for API.
**Example:**
```dockerfile
# syntax=docker/dockerfile:1

# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project files first (layer caching)
COPY ["src/InsuranceManager.Api/InsuranceManager.Api.csproj", "src/InsuranceManager.Api/"]
COPY ["src/InsuranceManager.Application/InsuranceManager.Application.csproj", "src/InsuranceManager.Application/"]
COPY ["src/InsuranceManager.Infrastructure/InsuranceManager.Infrastructure.csproj", "src/InsuranceManager.Infrastructure/"]
COPY ["src/InsuranceManager.Domain/InsuranceManager.Domain.csproj", "src/InsuranceManager.Domain/"]

RUN dotnet restore "src/InsuranceManager.Api/InsuranceManager.Api.csproj"

# Copy source and build
COPY src/ .
RUN dotnet build "src/InsuranceManager.Api/InsuranceManager.Api.csproj" -c Release -o /app/build

# Publish
FROM build AS publish
RUN dotnet publish "src/InsuranceManager.Api/InsuranceManager.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 2: Runtime (minimal)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Create non-root user for security
RUN adduser --disabled-password --gecos "" appuser && \
    chown -R appuser /app

USER appuser

COPY --from=publish /app/publish .

# Health check
HEALTHCHECK --interval=30s --timeout=3s --retries=3 \
  CMD curl -f http://localhost:8080/health || exit 1

# Expose port (ASPNETCORE_URLS env var controls this)
EXPOSE 8080

ENTRYPOINT ["dotnet", "InsuranceManager.Api.dll"]
```

### Pattern 2: Huey Worker Dockerfile
**What:** Python slim container with Huey and consumer script.
**When to use:** INFR-02: Huey worker runs as separate process/container.
**Example:**
```dockerfile
# syntax=docker/dockerfile:1

# Build stage for dependencies (optional - can skip for slimmer image)
FROM python:3.12-slim AS builder
RUN pip install --no-cache-dir huey requests

# Runtime stage
FROM python:3.12-slim AS runtime

# Install only production dependencies
COPY --from=builder /usr/local/lib/python3.12/site-packages /usr/local/lib/python3.12/site-packages
COPY --from=builder /usr/local/bin /usr/local/bin

# Copy Huey files
WORKDIR /app
COPY src/InsuranceManager.Application/Huey/ ./Huey/

# Create non-root user
RUN useradd -m worker && chown -R worker /app
USER worker

# Default command runs the consumer
CMD ["python", "Huey/huey_consumer.py"]
```

### Pattern 3: docker-compose.yml with Shared Volume
**What:** API and Huey worker containers share `./huey_data/` directory via bind mount.
**When to use:** INFR-03: Filesystem broker works on Windows and Docker.
**Example:**
```yaml
# docker-compose.yml
services:
  api:
    build:
      context: .
      dockerfile: src/InsuranceManager.Api/Dockerfile
    ports:
      - "5000:8080"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ASPNETCORE_URLS=http://+:8080
      - API_KEY=${API_KEY}  # From host env or .env file
      - ConnectionStrings__DefaultConnection=Data Source=insurance.db
    volumes:
      # Share Huey queue directory with worker
      - ./huey_data:/app/huey_data
    depends_on:
      - huey-worker
    restart: unless-stopped

  huey-worker:
    build:
      context: .
      dockerfile: Dockerfile.huey
    environment:
      - API_BASE_URL=http://api:8080
      - INTERNAL_API_KEY=${INTERNAL_API_KEY:-internal-secret-change-me}
    volumes:
      # Same volume mount as API - critical for FileHuey
      - ./huey_data:/app/huey_data
    depends_on:
      - api
    restart: unless-stopped

volumes:
  huey_data:
    driver: local
```

### Pattern 4: ASP.NET Core Health Endpoints
**What:** `/health` returns 200 when process is alive, `/ready` checks dependencies.
**When to use:** OBS-01/OBS-02 requirements (not in v1 scope, but should not break them).
**Example:**
```csharp
// In Program.cs or a dedicated configuration file
builder.Services.AddHealthChecks()
    .AddDbContextCheck<InsuranceDbContext>("database");

var app = builder.Build();

// Basic liveness - always returns 200 if process is running
app.MapHealthChecks("/health");

// Readiness - checks database and other dependencies
app.MapHealthChecks("/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready")
});
```

### Pattern 5: API Key Configuration via Environment
**What:** API Key read from environment variable, never baked into image.
**When to use:** AUTH-01/02 in Docker - secure configuration.
**Example:**
```json
// appsettings.json (DO NOT COMMIT SECRETS)
{
  "ApiKey": "${API_KEY}",  // ASP.NET Core reads from env var
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=insurance.db"
  }
}
```

Docker compose environment:
```yaml
environment:
  - API_KEY=${API_KEY}  # Resolves from host .env or environment
```

`.env` file (add to .gitignore):
```
API_KEY=your-secret-key-here
INTERNAL_API_KEY=internal-secret-change-me
```

## Don't Hand-Roll

| Problem | Don't Build | Use Instead | Why |
|---------|-----------|-------------|-----|
| Docker build process | Build .NET manually in container | Multi-stage Dockerfile with `dotnet publish` | Proper layer caching, smaller images |
| Python environment | Install full Python with all packages | Python slim + pip install only needed packages | Reduce image size, attack surface |
| Queue storage | Build custom file-based queue | Huey FileHuey | Phase 2 constraint; handles locking, retries, scheduling |
| File sharing between containers | Use network calls | Bind mounts | FileHuey requires filesystem access, bind mounts work on Windows+Docker |

**Key insight:** FileHuey uses exclusive file locks - this only works with Docker bind mounts, NOT CIFS/network shares. This was identified in Phase 2 research as a critical pitfall.

## Common Pitfalls

### Pitfall 1: FileHuey Over CIFS/Network Shares
**What goes wrong:** FileHuey tasks process fine locally but fail silently in Docker when using CIFS mounts.
**Why it happens:** FileHuey relies on exclusive file locks which don't work reliably over CIFS.
**How to avoid:** Use Docker bind mounts (`./huey_data:/app/huey_data`) in docker-compose.yml. Never use CIFS/network shares for Huey queue directory.
**Warning signs:** Tasks queue but don't process in Docker; logs show "file busy" or lock errors.

### Pitfall 2: API Key Baked into Docker Image
**What goes wrong:** API key committed to git via Dockerfile, rotated keys break production.
**Why it happens:** Developer adds `ENV API_KEY=secret` to Dockerfile instead of passing via environment.
**How to avoid:** Use `${API_KEY}` syntax in docker-compose.yml, store secrets in `.env` file (added to .gitignore).
**Warning signs:** `docker history` shows secrets in image layers.

### Pitfall 3: Wrong Port Configuration in Container
**What goes wrong:** API responds locally but not in container - port not exposed or ASPNETCORE_URLS wrong.
**Why it happens:** Default ASP.NET Core port 5000 doesn't work in containers; need 8080 or proper config.
**How to avoid:** Set `ASPNETCORE_URLS=http://+:8080` in docker-compose environment, expose 8080 in Dockerfile.
**Warning signs:** `curl localhost:5000` works locally but `docker exec curl localhost:5000` fails.

### Pitfall 4: Huey Worker Can't Reach API
**What goes wrong:** Worker container starts, can read queue, but HTTP calls to API fail.
**Why it happens:** Worker uses `localhost:5000` instead of Docker service name (`http://api:8080`).
**How to avoid:** Use Docker Compose service name as hostname (`API_BASE_URL=http://api:8080` in worker environment).
**Warning signs:** Worker logs show "Connection refused" or "Name or service not known".

### Pitfall 5: Volume Mount Path Mismatch
**What goes wrong:** Windows path `C:\project\huey_data` doesn't match container path `/app/huey_data`.
**Why it happens:** Developer configures volume mount incorrectly, FileHuey can't find queue files.
**How to avoid:** Use relative paths in docker-compose (`./huey_data:/app/huey_data`) - Docker Desktop handles Windows-to-Linux path translation.
**Warning signs:** FileHuey creates new queue in container filesystem instead of using shared host directory.

## Code Examples

### Environment Configuration in ASP.NET Core
```csharp
// Program.cs - read API key from environment
var apiKey = Environment.GetEnvironmentVariable("API_KEY")
    ?? configuration.GetValue<string>("ApiKey");

// Middleware receives IConfiguration which includes env vars
// so no code changes needed - it works automatically
```

### Huey Worker Health Check (Optional)
```python
# Could add a simple HTTP health endpoint to worker if needed
# For now, container health is sufficient
```

### Verify Bind Mount Works on Windows
```bash
# Test locally before Docker
docker run -v ./huey_data:/app/huey_data --rm python:3.12-slim ls /app/huey_data
# Should show empty or existing queue files from host
```

## State of the Art

| Old Approach | Current Approach | When Changed | Impact |
|--------------|-----------------|--------------|--------|
| No containerization | .NET + Python in Docker | Phase 3 | Deployable anywhere with Docker |
| Manual process management | docker-compose orchestration | Phase 3 | Consistent start/stop, health monitoring |
| Local-only Huey | Shared volume mount | Phase 3 | Same broker works in dev and Docker |

**Deprecated/outdated:**
- None for this phase

## Assumptions Log

| # | Claim | Section | Risk if Wrong |
|---|-------|---------|---------------|
| A1 | .NET 10 Docker images (sdk and aspnet) are available on MCR | Standard Stack | If images unavailable, need .NET 9 or build custom |
| A2 | FileHuey exclusive locks work over Docker bind mounts on Windows | Common Pitfalls | If locks don't work, need alternative broker |
| A3 | Huey worker can reach API via Docker service name | Common Pitfalls | If networking doesn't work, need network configuration |
| A4 | API is already running HTTP endpoint internally | Architecture | If API only listens on Unix socket, container networking different |

## Environment Availability

| Dependency | Required By | Available | Version | Fallback |
|------------|------------|-----------|---------|----------|
| Docker | Containerization | ✓ | 24.x+ | — |
| docker-compose | Multi-container orchestration | ✓ | 2.x+ | — |
| .NET 10 | API container build | ✓ | 10.0.103 | — |
| Python 3.12 | Huey worker container | ✓ | 3.12.7 | — |
| pip | Installing Python packages | ✓ | 25.3 | — |

**Missing dependencies with no fallback:**
- None identified - all tools available

**Missing dependencies with fallback:**
- Huey Python package → `pip install huey` in Dockerfile

## Security Domain

### Applicable ASVS Categories

| ASVS Category | Applies | Standard Control |
|---------------|---------|-----------------|
| V2 Authentication | yes | API Key via env var (not baked into image) |
| V3 Session Management | no | Stateless API Key auth - no sessions |
| V4 Access Control | no | Single API Key - no role-based access in v1 |
| V5 Input Validation | yes | ASP.NET Core model validation (existing) |
| V6 Cryptography | no | No encryption at rest in Phase 3 |

### Known Threat Patterns for Docker + Huey

| Pattern | STRIDE | Standard Mitigation |
|---------|--------|---------------------|
| Secrets in Docker image | Information Disclosure | Use environment variables, not ENV in Dockerfile |
| Container running as root | Elevation of Privilege | Create non-root user in Dockerfile (USER directive) |
| API Key in docker-compose.yml | Information Disclosure | Use .env file with ${VAR} syntax, add .env to .gitignore |
| Huey worker has too much access | Tampering | Worker only calls specific internal endpoint, limited permissions |

## Sources

### Primary (HIGH confidence)
- [Microsoft Learn: Health checks in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/health-checks?view=aspnetcore-10.0) - Official health check documentation
- [Docker: Bind mounts](https://docs.docker.com/engine/storage/bind-mounts/) - Bind mount behavior and constraints
- [Brady Stohler: Docker Best Practices for .NET Applications](https://www.bradystohler.com/notes/docker-dotnet-best-practices) - Multi-stage builds, health checks, security

### Secondary (MEDIUM confidence)
- [DZone: Deploying .NET 10 Applications Using Docker](https://dzone.com/articles/guide-deploying-dotnet-apps-with-docker) - .NET 10 containerization guide
- [Baeldung: Share Volume Between Multiple Containers](https://www.baeldung.com/ops/docker-share-volume-multiple-containers) - Volume sharing patterns
- [OneUptime: ASP.NET Core Health Checks](https://oneuptime.com/blog/post/2026-01-25-aspnet-core-health-checks/view) - Health check implementation

### Tertiary (LOW confidence)
- WebSearch for "FileHuey Docker Windows bind mount" - Cross-validated via Phase 2 research

## Validation Architecture

> Skip - `workflow.nyquist_validation` is explicitly `false` in config.json

## Metadata

**Confidence breakdown:**
- Standard Stack: HIGH - .NET 10 and Python 3.12 are project constraints, Docker images well-documented
- Architecture: HIGH - Pattern follows established multi-stage Docker + docker-compose patterns
- Pitfalls: HIGH - FileHuey CIFS issue documented in Phase 2 research, other pitfalls are common Docker mistakes

**Research date:** 2026-05-09
**Valid until:** 2026-06-08 (30 days — stable domain)