# Feature Research

**Domain:** Insurance Management API (Simplified)
**Researched:** 2026-05-09
**Confidence:** MEDIUM

## Feature Landscape

### Table Stakes (Users Expect These)

Features users assume exist. Missing these = product feels incomplete.

| Feature | Why Expected | Complexity | Notes |
|---------|--------------|------------|-------|
| **Proposal CRUD API** | Core domain — users expect to create, read, update, delete proposals via REST | LOW | Standard REST patterns, endpoints: `POST /proposals`, `GET /proposals`, `GET /proposals/{id}`, `PATCH /proposals/{id}` |
| **Policy Retrieval** | Once a proposal is approved, users expect to retrieve the generated policy | LOW | `GET /policies/{id}` with policy details including insured token |
| **Status Lifecycle Management** | Proposal flows through states (Em Análise → Aprovada/Recusada) — users expect to manage this | MEDIUM | State machine pattern with validation, `PATCH /proposals/{id}/status` |
| **API Authentication** | All insurance APIs require security — API key is minimum for service-to-service | LOW | Per PROJECT.md constraints: API Key only, no JWT |
| **REST API Documentation** | Developers expect OpenAPI/Swagger docs to understand endpoints | LOW | Swagger UI, OpenAPI 3.x spec at `/swagger` |
| **Proposal Listing with Filters** | Users need to find proposals by status, date range, etc. | LOW | `GET /proposals?status=EmAnalise&from=2026-01-01` |
| **Data Validation** | Invalid data must be rejected with clear error messages | LOW | Request validation, structured error responses (RFC 7807) |
| **Idempotent Operations** | Network retries happen — users expect same result | LOW | Operations should be idempotent or return existing resource |

### Differentiators (Competitive Advantage)

Features that set the product apart. Not required, but valuable.

| Feature | Value Proposition | Complexity | Notes |
|---------|-------------------|------------|-------|
| **Async Status Updates via Message Queue** | Status transitions happen via Huey queue — prevents blocking, enables retry on failure | MEDIUM | This is the core differentiation per PROJECT.md. Status changes queued, processed async. |
| **CQRS Read/Write Separation** | Query performance optimized — read models separate from write models | MEDIUM | Read adapters return projections optimized for queries, write adapters handle commands |
| **Webhook Notifications** | External systems can subscribe to proposal/policy events (created, status changed, policy bound) | MEDIUM | Event-driven integrations, "policy.bound" events for downstream systems |
| **Hexagonal Architecture with Isolated Adapters** | Vendor-agnostic persistence — swap SQLite for PostgreSQL without touching domain | HIGH | Ports and adapters pattern, domain has no DB dependencies |
| **State Transition Audit Trail** | Insurance requires traceability — who changed what and when | LOW | Store state transitions with timestamp, actor, previous/new state |
| **Health Check Endpoints** | Production integrations require monitoring — `/health`, `/ready` endpoints | LOW | Kubernetes/load balancer compatibility |
| **Request Correlation IDs** | Distributed tracing — trace requests through the queue and across services | LOW | Correlation ID header propagated through async processing |

### Anti-Features (Commonly Requested, Often Problematic)

Features that seem good but create problems.

| Feature | Why Requested | Why Problematic | Alternative |
|---------|---------------|-----------------|-------------|
| **Multi-Item Insured (Multiple tokens per policy)** | "What if user has multiple digital assets?" | Increases complexity significantly: item lifecycle, pricing per item, claims per item | Per PROJECT.md constraints: single 32-char token per policy. Add as separate entity if needed later. |
| **Claims Management** | "Insurance needs claims handling" | Claims is a domain onto itself: FNOL, adjudication, reserves, payments, subrogation | Explicitly out of scope per PROJECT.md. Build proposal→policy first, add claims in v2 if validated. |
| **JWT Authentication** | "More modern than API key" | Adds complexity: token refresh, audience/issuer validation, token revocation | API Key per constraints is sufficient for service-to-service. JWT only if multi-user scenario added. |
| **Multi-Carrier Support** | "Support multiple insurance carriers" | Multi-carrier introduces carrier-specific rules, commission handling, rate cards, binding authority | Single carrier focus for v1. Add carrier abstraction only if business need validated. |
| **Complex Rating Engine** | "Need premium calculation logic" | Rating engines are complex: actuarial tables, risk factors, coverage rules | Simplified v1: no rating, flat premium or external rating call. Add rating later when product defined. |
| **Real-Time Synchronous Status Updates** | "Want instant feedback when status changes" | Blocks API response, couples tightly to worker availability, no retry on failure | Async via Huey is better: immediate queue ack, background processing. Sync option could be added as v1.x feature. |
| **Full Policy Document Generation (PDF)** | "Need policy documents" | PDF generation adds significant complexity: templating, legal language, branding | Defer: simple JSON policy representation first. PDF could be v2 feature. |
| **Multi-Tenant Isolation** | "Support multiple organizations" | Adds tenant ID to every query, tenant-specific config, data isolation logic | Single-tenant for v1. Multi-tenant adds significant complexity — only if validated as requirement. |

## Feature Dependencies

```
Proposal Management (CRUD)
    └──requires──> Status Lifecycle
                       └──requires──> Policy Creation (from approved)
                                  └──requires──> Insured Token Generation

CQRS Read Models
    └──requires──> Proposal/Policy Entities

Async Status Updates (Huey)
    └──requires──> Proposal CRUD
                   └──requires──> Authentication

Webhook Events
    └──requires──> Policy Creation
                   └──requires──> Status Transitions
```

### Dependency Notes

- **Proposal CRUD requires Authentication:** All write operations need API key validation
- **Policy Creation requires Status Lifecycle:** Policy is created from approved proposal — the state machine drives this
- **CQRS requires Proposal/Policy Entities:** Read models are projections of the write models
- **Async Status Updates enhance Proposal CRUD:** Huey makes status changes non-blocking but wraps the base CRUD
- **Webhook Events enhance Policy Creation:** Events are emitted when policies are bound from approved proposals

## MVP Definition

### Launch With (v1)

Minimum viable product — what's needed to validate the concept.

- [x] **Proposal CRUD API** — Create, read, update proposals. Core domain interaction.
- [x] **Status Lifecycle** — Transitions from "Em Análise" to "Aprovada" or "Recusada" via async queue
- [x] **Policy Creation** — Automatic policy creation when approved proposal is "contratado"
- [x] **Proposal Listing** — Filter by status, date range
- [x] **Policy Retrieval** — Get policy by ID, includes insured token (32-char)
- [x] **API Key Authentication** — Simple auth for frontend/services
- [x] **SQLite Persistence** — Per constraints, adapter-agnostic by design
- [x] **CQRS Read Models** — Separate read adapters for query optimization

### Add After Validation (v1.x)

Features to add once core is working.

- [ ] **Webhook Events** — Push notifications on policy.bound, status changes. Triggers: when partner integrations validated.
- [ ] **Audit Trail** — Full state transition history. Triggers: when compliance needs identified.
- [ ] **Health/Ready Endpoints** — Production monitoring. Triggers: when deploying to staging.
- [ ] **Correlation IDs** — Request tracing. Triggers: when debugging distributed issues.

### Future Consideration (v2+)

Features to defer until product-market fit is established.

- [ ] Claims Management — Full FNOL, adjudication, payments
- [ ] Rating/Premium Calculation — Actuarial logic, risk factors
- [ ] PDF Policy Documents — Legal document generation
- [ ] Multi-Carrier Support — Carrier abstraction, binding authorities
- [ ] Multi-Tenant Isolation — Organization-level data separation
- [ ] JWT Authentication — User-level auth with roles/permissions

## Feature Prioritization Matrix

| Feature | User Value | Implementation Cost | Priority |
|---------|------------|---------------------|----------|
| Proposal CRUD | HIGH | LOW | P1 |
| Status Lifecycle (async via Huey) | HIGH | MEDIUM | P1 |
| Policy Creation (from approved) | HIGH | LOW | P1 |
| Policy Retrieval | HIGH | LOW | P1 |
| API Key Authentication | HIGH | LOW | P1 |
| CQRS Read Models | MEDIUM | MEDIUM | P1 |
| SQLite Persistence | HIGH | LOW | P1 |
| Webhook Events | MEDIUM | MEDIUM | P2 |
| Audit Trail | MEDIUM | LOW | P2 |
| Health Endpoints | MEDIUM | LOW | P2 |
| Correlation IDs | LOW | LOW | P2 |
| Claims Management | HIGH | HIGH | P3 (defer) |
| Rating Engine | MEDIUM | HIGH | P3 (defer) |
| PDF Documents | MEDIUM | MEDIUM | P3 (defer) |
| Multi-Carrier | MEDIUM | HIGH | P3 (defer) |
| JWT Auth | MEDIUM | MEDIUM | P3 (defer) |

**Priority key:**
- P1: Must have for launch
- P2: Should have, add when possible
- P3: Nice to have, future consideration

## Competitor Feature Analysis

| Feature | BindHQ | BriteCore | Boost | Our Approach |
|---------|--------|------------|-------|--------------|
| Full Policy Lifecycle | Full API | Full API | Full API | Simplified: proposal → policy only |
| Async Processing | Sync | Sync | Sync | **Differentiator:** Huey async status updates |
| Claims API | Integrated | Integrated | Full API | **Anti-feature:** Out of scope per constraints |
| CQRS Pattern | Not exposed | Not exposed | Not exposed | **Differentiator:** Explicit CQRS read/write isolation |
| Multi-Item Insured | Yes | Yes | Yes | **Anti-feature:** Single token per policy (simpler) |
| Authentication | OAuth2/OIDC | SAML/OIDC | API Key + OAuth2 | API Key only (simpler per constraints) |
| Architecture | Monolithic | Cloud-native | SaaS | **Differentiator:** Hexagonal/Ports & Adapters |

### Key Differentiation Strategy

1. **Async via Huey** — Competitors use synchronous APIs. Our async status updates via message queue is unique, enables retry, prevents blocking.
2. **CQRS Explicit** — Full read/write separation with isolated adapters. Competitors don't expose this pattern.
3. **Hexagonal Architecture** — Swappable persistence (SQLite now, PostgreSQL later without domain changes).
4. **Simplicity** — Single token insured, no claims, no rating. Competitors are full-suite. We win on focused simplicity.

## Sources

- BindHQ API documentation (api.bindhq.com) — Policy lifecycle API patterns
- BriteCore API documentation (api-docs.britecore.com) — Cloud-native P&C platform features
- Boost Insurance API Guide (learn.boostinsurance.com) — Full-cycle insurance API platform
- Quick Silver Systems Whitepaper (2025) — API-first insurance platforms, ACORD NGDS standards
- Genasys Tech (2026) — Insurance software feature trends
- Oracle Insurance Policy Administration REST API — Enterprise policy admin patterns
- Zywave API Ecosystem — Agency and policy management API patterns
- Briza API — Commercial insurance unified API patterns

---
*Feature research for: Insurance Manager API*
*Researched: 2026-05-09*