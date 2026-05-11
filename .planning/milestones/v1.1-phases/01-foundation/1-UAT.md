---
status: complete
updated: 2026-05-09T00:00:00Z
---

## Current Test

[testing complete]

## Tests

### 1. Create Proposal
expected: POST /api/proposals with client name and coverage type returns 201 Created with the created proposal including an auto-generated ID and status "EmAnalise"
result: pass

### 2. List All Proposals
expected: GET /api/proposals returns 200 OK with a list of all proposals
result: pass

### 3. Filter Proposals by Status
expected: GET /api/proposals?status=EmAnalise returns 200 OK with only proposals in "EmAnalise" status
result: pass

### 4. Get Proposal by ID
expected: GET /api/proposals/{id} returns 200 OK with the proposal details
result: pass

### 5. Contract Approved Proposal
expected: POST /api/proposals/{id}/contract on an approved proposal returns 201 Created and creates a policy with a 32-character asset token
result: pass

### 6. List All Policies
expected: GET /api/policies returns 200 OK with a list of all policies
result: pass

### 7. Get Policy by ID
expected: GET /api/policies/{id} returns 200 OK with the policy details including the 32-char asset token
result: pass

## Summary

total: 7
passed: 7
issues: 0
pending: 0
skipped: 0
blocked: 0

## Gaps

[none yet]