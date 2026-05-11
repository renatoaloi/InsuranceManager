# Phase 4: Huey Bugfixes - Research

**Gathered:** 2026-05-10
**Status:** Ready for planning
**Source:** v1.0 UAT findings + bug report

## Problem Statement

The Huey container is not starting correctly. Issues identified:
1. Container may not start or crashes immediately
2. Huey worker may not connect to filesystem broker correctly
3. Shared volume between API and Huey containers may not work

## Current Configuration Analysis

### Dockerfile.huey Issues

```
HUEY_QUEUE_PATH=/app/huey_data
CMD ["python", "-m", "huey", "-c", "Huey/huey_consumer.py", "worker"]
```

**Identified issues:**
1. CMD uses `-c Huey/huey_consumer.py` but COPY uses `./Huey/`
2. Module path resolution may fail inside container
3. `FileHuey` may have issues with directory permissions

### huey_consumer.py Issues

1. `huey_config` import in `run_worker.py` doesn't exist
2. Consumer is started but may not find the queue directory
3. FileHuey uses exclusive file locks that may conflict with API container

### docker-compose.yml Issues

1. Named volume `huey_data` conflicts with bind mount `./huey_data`
2. Both containers mount `./huey_data` (bind mount) and `huey_data` (named volume) which may cause confusion
3. Huey worker doesn't have a health check

## Solutions to Investigate

### 1. Fix Dockerfile CMD path
- Change `-c Huey/huey_consumer.py` to `-c Huey/huey_consumer.py` with correct working directory
- Or use `python Huey/run_worker.py` if that script exists

### 2. Fix FileHuey permissions
- Ensure huey_data directory exists and is writable
- Add `mkdir -p /app/huey_data` before starting worker

### 3. Fix docker-compose volume conflict
- Remove named volume or ensure bind mount is used consistently

### 4. Add Huey health check
- Add a script that checks if worker is alive
- Use `docker exec` to verify worker process is running

## Recommended Fixes

1. **Fix CMD in Dockerfile.huey:**
   - Use `CMD ["python", "Huey/huey_consumer.py"]` from WORKDIR /app
   - Or create an entrypoint script

2. **Add startup script:**
   - Create `entrypoint.sh` that creates directory and starts worker
   - Handle permissions for non-root user

3. **Fix volume in docker-compose:**
   - Use only bind mount `./huey_data:/app/huey_data`
   - Remove conflicting named volume

4. **Add logging:**
   - Verify worker is polling the queue
   - Check for file lock conflicts

## Files to Modify

1. `Dockerfile.huey` - Fix CMD and add startup logic
2. `docker-compose.yml` - Fix volume configuration
3. `src/InsuranceManager.Application/Huey/huey_consumer.py` - Fix queue path initialization
4. Optional: `src/InsuranceManager.Application/Huey/run_worker.py` - Remove if not used

## Testing Strategy

1. Build and run Huey container standalone
2. Check logs for worker startup
3. Verify queue directory is accessible
4. Test with API container via docker-compose
5. Verify task processing end-to-end