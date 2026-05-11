# Phase 4: huey-bugfixes - Context

**Gathered:** 2026-05-10
**Status:** Ready for planning

<domain>
## Phase Boundary

Fix Huey container startup issues and ensure worker processes tasks from filesystem queue (BUG-01, BUG-02, BUG-03).

</domain>

<decisions>
## Implementation Decisions

### Container Startup
- **D-01:** Use direct script approach instead of `python -m huey` module
  - New CMD: `CMD ["python", "Huey/huey_consumer.py"]`
  - File `huey_consumer.py` already has `if __name__ == '__main__':` block that runs Consumer directly

### Volume Sharing
- **D-02:** Both API and Huey containers use same path `/app/huey_data`
  - docker-compose bind mount: `./huey_data:/app/huey_data`
  - No separate paths needed - simpler configuration

### Debugging & Verification
- **D-03:** Use API endpoint trigger to verify Huey is processing tasks
  - Create test proposal via API
  - Trigger status change to enqueue task
  - Verify task processes through Huey queue

</decisions>

<canonical_refs>
## Canonical References

**Downstream agents MUST read these before planning or implementing.**

### Configuration Files
- `docker-compose.yml` — Container orchestration, volume mounts
- `Dockerfile.huey` — Huey worker container definition
- `src/InsuranceManager.Application/Huey/huey_consumer.py` — Worker implementation
- `src/InsuranceManager.Application/Huey/huey_config.py` — Huey configuration (note: not currently used by consumer - consider unifying)

### Requirements
- `.planning/REQUIREMENTS.md` — BUG-01, BUG-02, BUG-03 requirements
- `.planning/ROADMAP.md` — Phase 4 success criteria

</canonical_refs>

## Existing Code Insights

### Reusable Assets
- `huey_consumer.py`: Has `process_status_change` task that calls `/internal/status` endpoint
- `huey_config.py`: FileHuey configuration (currently not imported by consumer - inconsistency)

### Established Patterns
- FileHuey with filesystem broker - no Redis needed
- API Key auth via `X-API-Key` header
- Internal API endpoint `/internal/status` for worker communication

### Integration Points
- Huey worker calls API at `http://insurance-api:8080/internal/status`
- Both containers share `./huey_data` directory for queue files
- API writes queue entries, worker reads and processes them

</code_context>

<specifics>
## Specific Ideas

- User prefers direct script approach for running Huey (not module approach)
- User wants to test via API endpoint that triggers status change
- Keep existing bind mount approach for huey_data volume

</specifics>

<deferred>
## Deferred Ideas

None — discussion stayed within phase scope

</deferred>

---

*Phase: 4-huey-bugfixes*
*Context gathered: 2026-05-10*