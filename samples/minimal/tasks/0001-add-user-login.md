---
schema: 1
id: SAMPLE-0001
title: Add user login
status: Done
epic: E1-Auth
priority: P1
effort: M
assignee: founder
labels: [auth, backend]
dependencies: []
created: 2026-04-15
updated: 2026-05-01
---

## Description
Implement email + password login with PBKDF2 hashing.

## Acceptance Criteria
- [x] Email/password form
- [x] Argon2id hash storage
- [x] Session cookie issued on success
