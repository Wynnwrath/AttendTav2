---
id: 5
type: story
title: "Manual check-in fallback by name and student number"
parent: epic-check-in
covers: [R5, R8, R9]
after: [3, 4]
hitl: false
risk: medium
tracker_id: "48"
remote: "https://github.com/rainyDayssss/AttendTa/issues/48"
tracker_status: backlog
assignee: "rainyDayssss"
---

# Manual check-in fallback by name and student number

## Description

Allows Management to search Confirmed participants by name or student number with 25-row pagination (AD-13) in the scanner window (EXPERIENCE.md: manual lookup lives in the Scanner window) and execute check-in recording manual_search or student_number under the entry 3 validation and lock rules (AD-4), appending an attendance.recorded Activity Log entry (delivering R5, the manual audit part of R8, and the search paging part of R9).

## Requirements

Verify: An Officer searches confirmed participants by student number or name on an open event, sees a 25-row paged list, selects an attendee and clicks Check in, creating an Attendance Record with check_in_method manual_search or student_number; searching or viewing alone never inserts an attendance row.

## References

- parent — _bmad-output/initiative-attendta/tickets/epic-check-in/epic-check-in.md
