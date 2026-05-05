---
schema: 1
id: SAMPLE-0004
title: Set up CI pipeline
status: Done
epic: E4-Infra
priority: P0
effort: S
assignee: founder
labels: [devops, ci]
dependencies: []
created: 2026-04-10
updated: 2026-04-12
---

## Description
GitHub Actions workflow that runs build + test on every push and PR.

## Acceptance Criteria
- [x] Triggers on push to main
- [x] Triggers on pull_request
- [x] Caches NuGet packages
- [x] Uploads test result artifacts
