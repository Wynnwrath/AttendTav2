---
id: 3
type: story
title: "Paged participant detail rows with full-dataset totals"
parent: epic-participation-reports
covers: [R1, R2]
after: [2, 1.4, 1.6]
hitl: false
risk: low
tracker_id: "73"
remote: "https://github.com/rainyDayssss/AttendTa/issues/73"
tracker_status: backlog
---

# Paged participant detail rows with full-dataset totals

## Description

Extends the report view with server-side 25-row paged participant detail rows with FR-30 fields (name, student number, snapshot course, registration status, attendance result, Manila check-in time), ordered by participant name, then registration_id, so page boundaries are stable (AD-13), while preserving full-dataset summary totals and entry 2 attendee distributions across pages; needs the shared 25-row pager component from epic-platform-access.

## Requirements

Verify: Navigate through 25-row pages of participant detail rows on a completed event, verifying columns show name, student number, snapshot course, status, attendance result, and Manila check-in time, with stable paging across boundaries, while full-dataset totals and distributions remain unchanged.

## References

- parent — _bmad-output/initiative-attendta/tickets/epic-participation-reports/epic-participation-reports.md
