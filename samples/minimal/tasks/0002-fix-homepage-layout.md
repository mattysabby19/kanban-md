---
schema: 1
id: SAMPLE-0002
title: Fix homepage layout bug on mobile
status: InProgress
epic: E2-UX
priority: P0
effort: S
assignee: founder
labels:
- bug
- frontend
- mobile
dependencies: []
created: 2026-05-02
updated: 2026-05-05
---

## Description
On screens narrower than 380px, the hero CTA wraps onto its own line and the
margin collapses, leaving an awkward gap. Reproduce on iPhone SE.

## Acceptance Criteria
- [ ] CTA stays on one line down to 320px
- [ ] No vertical gap above the fold
- [ ] Verified on Chrome, Safari, Firefox mobile emulators
